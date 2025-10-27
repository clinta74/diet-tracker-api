global using System;
global using System.Collections.Generic;
global using MediatR;

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
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
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

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblyContaining<Program>());

// Database configuration
var dataSource = builder.Configuration["DATA_SOURCE"];
var initialCatalog = builder.Configuration["INITIAL_CATALOG"];
var dbPassword = builder.Configuration["DB_PASSWORD"];
var userID = builder.Configuration["DB_USERNAME"];

var sqlBuilder = new SqlConnectionStringBuilder()
{
    DataSource = dataSource,
    InitialCatalog = initialCatalog,
    Password = dbPassword,
    UserID = userID,
    IntegratedSecurity = false,
    TrustServerCertificate = true,
};

builder.Services.AddDbContext<DietTrackerDbContext>(options =>
{
    options.UseSqlServer(sqlBuilder.ConnectionString);
}, ServiceLifetime.Transient);

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

builder.Services.AddTransient<IAuth0ManagementApiClient>(provider => new Auth0ManagementApiClient(apiClientId, clientSecret, builder.Configuration["Auth0:Domain"]));

builder.Services.AddScoped<UserExistsFilter>();

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
    .WithOrigins(
    [
        "http://localhost:4000",
        "https://food.pollyspeople.net",
        "https://app.yourmealtracker.com",
    ])
    .AllowAnyMethod()
    .AllowAnyHeader());

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
