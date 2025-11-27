global using System;
global using System.Collections.Generic;
global using Mediator;

using System.Security.Claims;
using System.Text.Json.Serialization;
using diet_tracker_api.Authorization;
using diet_tracker_api.DataLayer;
using diet_tracker_api.Filters;
using diet_tracker_api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Npgsql;
using Swashbuckle.AspNetCore.SwaggerUI;

var builder = WebApplication.CreateBuilder(args);

var domain = $"https://{builder.Configuration["Auth0:Domain"]}/";
var audience = builder.Configuration["Auth0:Audience"];

builder.Services.AddControllers(config =>
{
    config.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer()));
    config.Filters.Add<OperationCancelledExceptionFilter>();
})
.AddControllersAsServices()
.AddJsonOptions(options =>
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddHttpContextAccessor();

builder.Services.AddMediator(options =>
{
    options.ServiceLifetime = ServiceLifetime.Transient;
});

// Database configuration
var host = builder.Configuration["DB_HOST"] ?? throw new InvalidOperationException("DB_HOST configuration is required");
var port = builder.Configuration["DB_PORT"] ?? "5432";
var database = builder.Configuration["DB_NAME"] ?? throw new InvalidOperationException("DB_NAME configuration is required");
var username = builder.Configuration["DB_USERNAME"] ?? throw new InvalidOperationException("DB_USERNAME configuration is required");
var password = builder.Configuration["DB_PASSWORD"] ?? throw new InvalidOperationException("DB_PASSWORD configuration is required");

if (!int.TryParse(port, out var portNumber))
{
    throw new InvalidOperationException($"DB_PORT must be a valid integer. Received: {port}");
}

var connectionBuilder = new NpgsqlConnectionStringBuilder
{
    Host = host,
    Port = portNumber,
    Database = database,
    Username = username,
    Password = password
};

builder.Services.AddDbContext<DietTrackerDbContext>(options =>
{
    options.UseNpgsql(connectionBuilder.ConnectionString, 
        npgsqlOptions => npgsqlOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery));
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "diet_tracker_api", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            Implicit = new OpenApiOAuthFlow
            {
                Scopes = new Dictionary<string, string>
                {
                    { "openid", "Open Id" }
                },
                AuthorizationUrl = new Uri(domain + "authorize?audience=" + audience)
            }
        }
    });
    c.OperationFilter<SecurityRequirementsOperationFilter>();
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.Authority = domain;
    options.Audience = audience;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ClockSkew = TimeSpan.FromSeconds(5),
        NameClaimType = ClaimTypes.NameIdentifier
    };
});

builder.Services.AddSingleton<IAuthorizationHandler, HasScopeHandler>();

var permissions = new string[] { "write:fuelings", "write:plans", "write:lean-and-greens", "write:user", "read:user:fuelings", "read:user:lean-and-green" };

builder.Services.AddAuthorization(options =>
{
    foreach (var permission in permissions)
    {
        options.AddPolicy(permission, policy => policy.Requirements.Add(new HasScopeRequirement(permission, domain)));
    }
});

var clientSecret = builder.Configuration["Auth0:ClientSecret"];
var apiClientId = builder.Configuration["Auth0:ApiClientId"];
var auth0Domain = builder.Configuration["Auth0:Domain"];

builder.Services.AddTransient<IAuth0ManagementApiClient>(provider => 
    Auth0ManagementApiClient.CreateAsync(apiClientId, clientSecret, auth0Domain).GetAwaiter().GetResult());

builder.Services.AddScoped<UserExistsFilter>();

// Add health checks
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionBuilder.ConnectionString, name: "database");

var app = builder.Build();

// Update database
using (var serviceScope = app.Services.GetRequiredService<IServiceScopeFactory>().CreateScope())
{
    using (var context = serviceScope.ServiceProvider.GetRequiredService<DietTrackerDbContext>())
    {
        context.Database.Migrate();
    }
}

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "diet_tracker_api v1");
    c.OAuthClientId(builder.Configuration["Auth0:ClientId"]);
    c.DefaultModelRendering(ModelRendering.Example);
    c.DefaultModelExpandDepth(1);
});

app.UseCors(config => config
    .WithExposedHeaders("x-total-count")
    .WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? new[] { "http://localhost:4000" })
    .AllowAnyMethod()
    .AllowAnyHeader());

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
