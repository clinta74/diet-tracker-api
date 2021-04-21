using System.Security.Claims;
using diet_tracker_api.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authorization;
using diet_tracker_api.Filters;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using diet_tracker_api.DataLayer;
using Auth0.ManagementApi;
using diet_tracker_api.Services;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Http;
using System.Security.Principal;

namespace diet_tracker_api
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var domain = $"https://{Configuration["Auth0:Domain"]}/";
            var audience = Configuration["Auth0:Audience"];

            services.AddControllers(config =>
            {
                config.Filters.Add<OperationCancelledExceptionFilter>();
            });

            services.AddHttpContextAccessor();

            /**
            * Setup the database configuration.
            */
            var dbPassword = Configuration["DB_PASSWORD"];
            var userID = Configuration["DB_USERNAME"];

            var builder = new SqlConnectionStringBuilder(Configuration.GetConnectionString("TempTrackerDatabase"))
            {
                Password = dbPassword,
                UserID = userID,
                IntegratedSecurity = false,
            };

            services.AddDbContext<DietTrackerDbContext>(options =>
            {
                options.UseSqlServer(builder.ConnectionString);
            }, ServiceLifetime.Transient);

            services.AddSwaggerGen(c =>
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

            services.AddAuthentication(options =>
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
                        NameClaimType = ClaimTypes.NameIdentifier
                    };
                });

            services.AddSingleton<IAuthorizationHandler, HasScopeHandler>();

            services.AddAuthorization(options =>
                {
                    options.AddPolicy("read:fuelings", policy => policy.Requirements.Add(new HasScopeRequirement("read:fuelings", domain)));
                });

            var clientSecret = Configuration["Auth0:ClientSecret"];
            var apiClientId = Configuration["Auth0:ApiClientId"];

            services.AddTransient<IAuth0ManagementApiClient>(provider => new Auth0ManagementApiClient(apiClientId, clientSecret, Configuration["Auth0:Domain"]));
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            UpdateDatabase(app);

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "diet_tracker_api v1");
                    c.OAuthClientId(Configuration["Auth0:ClientId"]);
                });
            }

            app.UseHttpsRedirection();

            app.UseCors(config => config
                .WithExposedHeaders("x-total-count")
                .WithOrigins(new string[]
                {
                    "http://localhost:4000",
                    "https://diet-tracker.pollyspeople.net"
                })
                .AllowAnyMethod()
                .AllowAnyHeader());

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

        private static void UpdateDatabase(IApplicationBuilder app)
        {
            using (var serviceScope = app.ApplicationServices
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                using (var context = serviceScope.ServiceProvider.GetService<DietTrackerDbContext>())
                {
                    context.Database.Migrate();
                }
            }
        }
    }
}
