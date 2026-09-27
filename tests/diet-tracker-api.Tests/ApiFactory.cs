using System.Net.Http.Headers;
using System.Net.Http.Json;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Testcontainers.PostgreSql;

namespace diet_tracker_api.Tests;

public record AuthResponse(string AccessToken, string RefreshToken, int ExpiresIn);

/// <summary>
/// Hosts the API in-memory against a throwaway PostgreSQL container.
/// On startup it seeds one plan and registers an admin user, so every user a test
/// registers afterwards gets the default (non-admin) permissions.
/// </summary>
public sealed class ApiFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    public const string Issuer = "https://tests.diet-tracker.local";

    private readonly PostgreSqlContainer _postgres = new PostgreSqlBuilder("postgres:17").Build();

    public CapturingLoggerProvider Logs { get; } = new();
    public int PlanId { get; private set; }
    public AuthResponse Admin { get; private set; } = null!;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureLogging(logging => logging.AddProvider(Logs));
        builder.UseSetting("DB_HOST", _postgres.Hostname);
        builder.UseSetting("DB_PORT", _postgres.GetMappedPublicPort(PostgreSqlBuilder.PostgreSqlPort).ToString());
        builder.UseSetting("DB_NAME", PostgreSqlBuilder.DefaultDatabase);
        builder.UseSetting("DB_USERNAME", PostgreSqlBuilder.DefaultUsername);
        builder.UseSetting("DB_PASSWORD", PostgreSqlBuilder.DefaultPassword);
        builder.UseSetting("Jwt:Issuer", Issuer);
        builder.UseSetting("Jwt:Audience", Issuer);
        builder.UseSetting("Jwt:SecretKey", "integration-test-secret-key-at-least-32-chars");
    }

    public async ValueTask InitializeAsync()
    {
        await _postgres.StartAsync();

        // Creating the server runs the startup migrations.
        using (var scope = Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<DietTrackerDbContext>();
            var plan = new Plan { Name = "Test Plan", FuelingCount = 5, MealCount = 1 };
            db.Plans.Add(plan);
            await db.SaveChangesAsync();
            PlanId = plan.PlanId;
        }

        Admin = await RegisterAsync(CreateClient(), NewEmail(), "Admin-Passw0rd!");
    }

    public static string NewEmail() => $"user-{Guid.NewGuid():N}@example.com";

    public async Task<AuthResponse> RegisterAsync(HttpClient client, string email, string password)
    {
        var response = await client.PostAsJsonAsync("/api/auth/register",
            new { FirstName = "Test", LastName = "User", Email = email, Password = password, PlanId });
        response.EnsureSuccessStatusCode();
        return (await response.Content.ReadFromJsonAsync<AuthResponse>())!;
    }

    /// <summary>Registers a new (non-admin) user and returns a client authenticated as them.</summary>
    public async Task<HttpClient> CreateUserClientAsync()
    {
        var auth = await RegisterAsync(CreateClient(), NewEmail(), "Correct-Horse-9");
        return CreateAuthorizedClient(auth.AccessToken);
    }

    public HttpClient CreateAuthorizedClient(string accessToken)
    {
        var client = CreateClient();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        return client;
    }

    public override async ValueTask DisposeAsync()
    {
        await base.DisposeAsync();
        await _postgres.DisposeAsync();
    }
}
