using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace diet_tracker_api.Tests;

/// <summary>
/// Requests with omitted/null optional fields that used to throw NullReferenceException (500).
/// </summary>
public class NullHandlingTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private async Task<HttpClient> CreateUserClientAsync()
    {
        var auth = await factory.RegisterAsync(factory.CreateClient(), ApiFactory.NewEmail(), "Correct-Horse-9");
        return factory.CreateAuthorizedClient(auth.AccessToken);
    }

    [Fact]
    public async Task Day_fuelings_with_null_names_are_skipped()
    {
        var client = await CreateUserClientAsync();
        const string day = "2026-09-01";
        var createDay = await client.PutAsJsonAsync($"/api/day/{day}", new { Water = 1, Weight = 0 }, Ct);
        Assert.Equal(HttpStatusCode.OK, createDay.StatusCode);

        var response = await client.PutAsJsonAsync($"/api/day/{day}/fuelings", new object[]
        {
            new { UserFuelingId = 0, Name = (string?)null, When = (DateTime?)null },
            new { UserFuelingId = 0, Name = "Bar", When = (DateTime?)null },
        }, Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        // The response pads unused slots up to the plan's fueling count with empty names.
        var fuelings = await response.Content.ReadFromJsonAsync<JsonElement>(Ct);
        var named = fuelings.EnumerateArray().Select(f => f.GetProperty("name").GetString()).Where(n => !string.IsNullOrEmpty(n));
        Assert.Equal(["Bar"], named);
    }

    [Fact]
    public async Task Tracking_values_without_metadata_are_accepted()
    {
        var client = await CreateUserClientAsync();

        var created = await client.PostAsJsonAsync("/api/user-tracking", new
        {
            Title = "Steps",
            Description = "Daily steps",
            Occurrences = 1,
            Order = 0,
            Disabled = false,
            UseTime = false,
            Values = new[] { new { UserTrackingValueId = 0, Name = "Count", Description = "Steps taken", Type = "Number", Order = 0, Disabled = false } },
        }, Ct);
        Assert.Equal(HttpStatusCode.OK, created.StatusCode);
        var tracking = await created.Content.ReadFromJsonAsync<JsonElement>(Ct);
        var trackingId = tracking.GetProperty("userTrackingId").GetInt32();
        var valueId = tracking.GetProperty("values")[0].GetProperty("userTrackingValueId").GetInt32();

        var updated = await client.PutAsJsonAsync($"/api/user-tracking/{trackingId}", new
        {
            Title = "Steps",
            Description = "Daily steps",
            Occurrences = 1,
            Order = 0,
            Disabled = false,
            UseTime = false,
            Values = new[] { new { UserTrackingValueId = valueId, Name = "Total", Description = "Steps taken", Type = "Number", Order = 0, Disabled = false } },
        }, Ct);

        Assert.True(updated.IsSuccessStatusCode, $"Unexpected {(int)updated.StatusCode}");
    }

    [Fact]
    public async Task New_user_without_names_is_rejected()
    {
        var client = await CreateUserClientAsync();

        var response = await client.PostAsJsonAsync("/api/new-user", new { EmailAddress = "x@example.com", PlanId = factory.PlanId }, Ct);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
