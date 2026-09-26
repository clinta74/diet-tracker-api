using System.Net;
using System.Net.Http.Json;
using diet_tracker_api.DataLayer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace diet_tracker_api.Tests;

public class PasswordHashingTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private const string Password = "Correct-Horse-9";
    private readonly HttpClient _client = factory.CreateClient();
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task New_passwords_are_not_stored_as_bcrypt()
    {
        var email = ApiFactory.NewEmail();
        await factory.RegisterAsync(_client, email, Password);

        var hash = await GetStoredHashAsync(email);

        Assert.False(IsBcrypt(hash));
    }

    [Fact]
    public async Task Legacy_bcrypt_hash_is_upgraded_on_successful_login()
    {
        var email = ApiFactory.NewEmail();
        await factory.RegisterAsync(_client, email, Password);
        await SetStoredHashAsync(email, BCrypt.Net.BCrypt.HashPassword(Password));

        var wrongPassword = await _client.PostAsJsonAsync("/api/auth/login", new { Email = email, Password = "nope" }, Ct);
        Assert.Equal(HttpStatusCode.Unauthorized, wrongPassword.StatusCode);
        Assert.True(IsBcrypt(await GetStoredHashAsync(email)));

        var firstLogin = await _client.PostAsJsonAsync("/api/auth/login", new { Email = email, Password }, Ct);
        Assert.Equal(HttpStatusCode.OK, firstLogin.StatusCode);
        Assert.False(IsBcrypt(await GetStoredHashAsync(email)));

        var secondLogin = await _client.PostAsJsonAsync("/api/auth/login", new { Email = email, Password }, Ct);
        Assert.Equal(HttpStatusCode.OK, secondLogin.StatusCode);
    }

    [Fact]
    public async Task Legacy_bcrypt_hash_still_verifies_for_password_change()
    {
        var email = ApiFactory.NewEmail();
        var auth = await factory.RegisterAsync(_client, email, Password);
        await SetStoredHashAsync(email, BCrypt.Net.BCrypt.HashPassword(Password));

        var changed = await factory.CreateAuthorizedClient(auth.AccessToken).PutAsJsonAsync("/api/account/password",
            new { CurrentPassword = Password, NewPassword = "Battery-Staple-7" }, Ct);

        Assert.Equal(HttpStatusCode.NoContent, changed.StatusCode);
        Assert.False(IsBcrypt(await GetStoredHashAsync(email)));
    }

    private static bool IsBcrypt(string hash) => hash.StartsWith("$2", StringComparison.Ordinal);

    private async Task<string> GetStoredHashAsync(string email)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DietTrackerDbContext>();
        return await db.UserCredentials
            .Where(c => c.Email == email)
            .Select(c => c.PasswordHash)
            .SingleAsync(Ct);
    }

    private async Task SetStoredHashAsync(string email, string hash)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DietTrackerDbContext>();
        await db.UserCredentials
            .Where(c => c.Email == email)
            .ExecuteUpdateAsync(setters => setters.SetProperty(c => c.PasswordHash, hash), Ct);
    }
}
