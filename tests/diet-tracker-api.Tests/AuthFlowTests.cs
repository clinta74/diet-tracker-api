using System.Net;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace diet_tracker_api.Tests;

/// <summary>
/// Baseline behaviour of the native auth flows. These must keep passing across the
/// .NET 10 upgrade and the password-hashing / JWT library replacements.
/// </summary>
public class AuthFlowTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private const string Password = "Correct-Horse-9";
    private readonly HttpClient _client = factory.CreateClient();
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    [Fact]
    public async Task Health_endpoint_reports_healthy()
    {
        var response = await _client.GetAsync("/health", Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync(Ct));
    }

    [Fact]
    public async Task Swagger_document_describes_bearer_security()
    {
        var document = await _client.GetFromJsonAsync<JsonElement>("/swagger/v1/swagger.json", Ct);

        Assert.Equal("diet_tracker_api", document.GetProperty("info").GetProperty("title").GetString());
        var scheme = document.GetProperty("components").GetProperty("securitySchemes").GetProperty("Bearer");
        Assert.Equal("bearer", scheme.GetProperty("scheme").GetString());

        var paths = document.GetProperty("paths");
        var getPlans = paths.GetProperty("/api/plans").GetProperty("get");
        Assert.True(getPlans.GetProperty("security")[0].TryGetProperty("Bearer", out _));
        Assert.True(getPlans.GetProperty("responses").TryGetProperty("401", out _));
        Assert.False(paths.GetProperty("/api/auth/login").GetProperty("post").TryGetProperty("security", out _));
    }

    [Fact]
    public async Task Swagger_ui_is_served()
    {
        var response = await _client.GetAsync("/swagger/index.html", Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Register_issues_tokens_with_name_and_default_permissions()
    {
        var auth = await factory.RegisterAsync(_client, ApiFactory.NewEmail(), Password);

        Assert.False(string.IsNullOrEmpty(auth.AccessToken));
        Assert.False(string.IsNullOrEmpty(auth.RefreshToken));
        Assert.Equal(15 * 60, auth.ExpiresIn);

        var payload = DecodePayload(auth.AccessToken);
        // The server reads the user id from this claim; keep its name stable across token handler changes.
        Assert.True(payload.TryGetProperty("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier", out var userId));
        Assert.False(string.IsNullOrEmpty(userId.GetString()));
        Assert.Equal("Test User", payload.GetProperty("name").GetString());
        Assert.Equal(ApiFactory.Issuer, payload.GetProperty("iss").GetString());
        var permissions = payload.GetProperty("permissions").EnumerateArray().Select(p => p.GetString()).ToList();
        Assert.Contains("write:user", permissions);
        Assert.DoesNotContain("admin:users", permissions);
    }

    [Fact]
    public async Task Login_succeeds_with_correct_password_and_fails_otherwise()
    {
        var email = ApiFactory.NewEmail();
        await factory.RegisterAsync(_client, email, Password);

        var ok = await _client.PostAsJsonAsync("/api/auth/login", new { Email = email.ToUpperInvariant(), Password }, Ct);
        var wrongPassword = await _client.PostAsJsonAsync("/api/auth/login", new { Email = email, Password = "nope" }, Ct);
        var unknownEmail = await _client.PostAsJsonAsync("/api/auth/login", new { Email = ApiFactory.NewEmail(), Password }, Ct);

        Assert.Equal(HttpStatusCode.OK, ok.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, wrongPassword.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, unknownEmail.StatusCode);
    }

    [Fact]
    public async Task Access_token_authenticates_the_current_user()
    {
        var auth = await factory.RegisterAsync(_client, ApiFactory.NewEmail(), Password);

        var anonymous = await _client.GetAsync("/api/user", Ct);
        var authorized = await factory.CreateAuthorizedClient(auth.AccessToken).GetAsync("/api/user", Ct);

        Assert.Equal(HttpStatusCode.Unauthorized, anonymous.StatusCode);
        Assert.Equal(HttpStatusCode.OK, authorized.StatusCode);
        var user = await authorized.Content.ReadFromJsonAsync<JsonElement>(Ct);
        Assert.Equal("Test", user.GetProperty("firstName").GetString());
    }

    [Fact]
    public async Task Permission_policies_are_enforced()
    {
        var user = await factory.RegisterAsync(_client, ApiFactory.NewEmail(), Password);

        var asUser = await factory.CreateAuthorizedClient(user.AccessToken).GetAsync("/api/admin/users", Ct);
        var asAdmin = await factory.CreateAuthorizedClient(factory.Admin.AccessToken).GetAsync("/api/admin/users", Ct);

        Assert.Equal(HttpStatusCode.Forbidden, asUser.StatusCode);
        Assert.Equal(HttpStatusCode.OK, asAdmin.StatusCode);
    }

    [Fact]
    public async Task Refresh_rotates_the_token_and_rejects_reuse()
    {
        var auth = await factory.RegisterAsync(_client, ApiFactory.NewEmail(), Password);

        var first = await _client.PostAsJsonAsync("/api/auth/refresh", new { auth.RefreshToken }, Ct);
        var reused = await _client.PostAsJsonAsync("/api/auth/refresh", new { auth.RefreshToken }, Ct);

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        var rotated = (await first.Content.ReadFromJsonAsync<AuthResponse>(Ct))!;
        Assert.NotEqual(auth.RefreshToken, rotated.RefreshToken);
        Assert.Equal(HttpStatusCode.Unauthorized, reused.StatusCode);

        var next = await _client.PostAsJsonAsync("/api/auth/refresh", new { rotated.RefreshToken }, Ct);
        Assert.Equal(HttpStatusCode.OK, next.StatusCode);
    }

    [Fact]
    public async Task Revoked_refresh_token_cannot_be_used()
    {
        var auth = await factory.RegisterAsync(_client, ApiFactory.NewEmail(), Password);
        var client = factory.CreateAuthorizedClient(auth.AccessToken);

        var revoke = await client.PostAsJsonAsync("/api/auth/revoke", new { auth.RefreshToken }, Ct);
        var refresh = await _client.PostAsJsonAsync("/api/auth/refresh", new { auth.RefreshToken }, Ct);

        Assert.Equal(HttpStatusCode.NoContent, revoke.StatusCode);
        Assert.Equal(HttpStatusCode.Unauthorized, refresh.StatusCode);
    }

    [Fact]
    public async Task Change_password_requires_current_password_and_replaces_it()
    {
        var email = ApiFactory.NewEmail();
        var auth = await factory.RegisterAsync(_client, email, Password);
        var client = factory.CreateAuthorizedClient(auth.AccessToken);
        const string newPassword = "Battery-Staple-7";

        var wrongCurrent = await client.PutAsJsonAsync("/api/account/password",
            new { CurrentPassword = "nope", NewPassword = newPassword }, Ct);
        var changed = await client.PutAsJsonAsync("/api/account/password",
            new { CurrentPassword = Password, NewPassword = newPassword }, Ct);

        Assert.Equal(HttpStatusCode.Unauthorized, wrongCurrent.StatusCode);
        Assert.Equal(HttpStatusCode.NoContent, changed.StatusCode);

        var oldLogin = await _client.PostAsJsonAsync("/api/auth/login", new { Email = email, Password }, Ct);
        var newLogin = await _client.PostAsJsonAsync("/api/auth/login", new { Email = email, Password = newPassword }, Ct);
        Assert.Equal(HttpStatusCode.Unauthorized, oldLogin.StatusCode);
        Assert.Equal(HttpStatusCode.OK, newLogin.StatusCode);
    }

    [Fact]
    public async Task Admin_can_set_user_credentials()
    {
        var email = ApiFactory.NewEmail();
        var auth = await factory.RegisterAsync(_client, email, Password);
        var me = await factory.CreateAuthorizedClient(auth.AccessToken).GetFromJsonAsync<JsonElement>("/api/user", Ct);
        var userId = me.GetProperty("userId").GetString();
        const string resetPassword = "Admin-Reset-5";

        var reset = await factory.CreateAuthorizedClient(factory.Admin.AccessToken)
            .PostAsJsonAsync($"/api/admin/users/{userId}/credentials", new { Email = email, Password = resetPassword }, Ct);

        Assert.True(reset.IsSuccessStatusCode, $"Unexpected {(int)reset.StatusCode}");
        var newLogin = await _client.PostAsJsonAsync("/api/auth/login", new { Email = email, Password = resetPassword }, Ct);
        Assert.Equal(HttpStatusCode.OK, newLogin.StatusCode);
    }

    [Fact]
    public async Task Legacy_migrate_endpoint_is_gone()
    {
        var response = await _client.PostAsJsonAsync("/api/auth/migrate",
            new { Email = ApiFactory.NewEmail(), NewPassword = Password }, Ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static JsonElement DecodePayload(string jwt)
    {
        var payload = jwt.Split('.')[1].Replace('-', '+').Replace('_', '/');
        payload = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
        return JsonDocument.Parse(Encoding.UTF8.GetString(Convert.FromBase64String(payload))).RootElement;
    }
}
