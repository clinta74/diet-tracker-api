using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace diet_tracker_api.Tests;

/// <summary>
/// Custom tracking items: every write is limited to the requesting user's trackings and values.
/// List updates prune foreign ids (with a warning); single-resource calls return 404.
/// </summary>
public class TrackingOwnershipTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private record Metadata(string Key, string? Value);
    private record Value(int UserTrackingValueId, string? Name, Metadata[]? Metadata = null, string Type = "Number", int Order = 0, bool Disabled = false);
    private record Tracking(int UserTrackingId, string? Title, List<Value> Values);

    private async Task<HttpClient> CreateUserClientAsync()
    {
        var auth = await factory.RegisterAsync(factory.CreateClient(), ApiFactory.NewEmail(), "Correct-Horse-9");
        return factory.CreateAuthorizedClient(auth.AccessToken);
    }

    private static object TrackingBody(string title, params Value[] values) => new
    {
        Title = title,
        Description = "",
        Occurrences = 1,
        Order = 0,
        Disabled = false,
        UseTime = false,
        Values = values,
    };

    private static async Task<Tracking> CreateTrackingAsync(HttpClient client, string title, params Value[] values)
    {
        var response = await client.PostAsJsonAsync("/api/user-tracking", TrackingBody(title, values), Ct);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<Tracking>(Ct))!;
    }

    private static async Task<Tracking> GetTrackingAsync(HttpClient client, int userTrackingId) =>
        (await client.GetFromJsonAsync<Tracking>($"/api/user-tracking/{userTrackingId}", Ct))!;

    private bool WarningLogged(string prefix, int id) => factory.Logs.Entries.Any(log =>
        log.Level == LogLevel.Warning && log.Message.StartsWith(prefix) && log.Message.EndsWith($": {id}"));

    [Fact]
    public async Task Reading_another_users_tracking_finds_nothing()
    {
        var owner = await CreateUserClientAsync();
        var other = await CreateUserClientAsync();
        var tracking = await CreateTrackingAsync(owner, "Steps", new Value(0, "Count"));

        var byId = await other.GetAsync($"/api/user-tracking/{tracking.UserTrackingId}", Ct);
        var values = await other.GetFromJsonAsync<List<Value>>($"/api/user-tracking-values/user-tracking/{tracking.UserTrackingId}", Ct);
        var history = await other.GetFromJsonAsync<List<JsonElement>>($"/api/day-tracking-values/{tracking.UserTrackingId}/history?startDate=2026-01-01", Ct);

        Assert.Equal(HttpStatusCode.NotFound, byId.StatusCode);
        Assert.Empty(values!);
        Assert.Empty(history!);
    }

    [Fact]
    public async Task Adding_a_value_to_another_users_tracking_is_not_found()
    {
        var owner = await CreateUserClientAsync();
        var other = await CreateUserClientAsync();
        var tracking = await CreateTrackingAsync(owner, "Steps", new Value(0, "Count"));

        var response = await other.PostAsJsonAsync("/api/user-tracking-value",
            new { tracking.UserTrackingId, Name = "Injected", Type = "Number" }, Ct);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal(["Count"], (await GetTrackingAsync(owner, tracking.UserTrackingId)).Values.Select(v => v.Name));
    }

    [Fact]
    public async Task Values_can_be_added_updated_and_deleted_on_own_tracking()
    {
        var client = await CreateUserClientAsync();
        var tracking = await CreateTrackingAsync(client, "Sleep", new Value(0, "Hours"));

        var added = await client.PostAsJsonAsync("/api/user-tracking-value",
            new { tracking.UserTrackingId, Name = "Quality", Type = "Number", Metadata = new[] { new Metadata("scale", "5") } }, Ct);
        Assert.Equal(HttpStatusCode.OK, added.StatusCode);
        var addedId = await added.Content.ReadFromJsonAsync<int>(Ct);

        var updated = await client.PutAsJsonAsync($"/api/user-tracking-value/{addedId}",
            new { tracking.UserTrackingId, Name = "Restfulness", Type = "Number", Metadata = new[] { new Metadata("scale", "10") } }, Ct);
        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);

        var value = (await GetTrackingAsync(client, tracking.UserTrackingId)).Values.Single(v => v.UserTrackingValueId == addedId);
        Assert.Equal("Restfulness", value.Name);
        Assert.Equal([new Metadata("scale", "10")], value.Metadata);

        var deleted = await client.DeleteAsync($"/api/user-tracking-value/{addedId}", Ct);
        Assert.Equal(HttpStatusCode.OK, deleted.StatusCode);
        Assert.Equal(["Hours"], (await GetTrackingAsync(client, tracking.UserTrackingId)).Values.Select(v => v.Name));
    }

    [Fact]
    public async Task Updating_or_deleting_another_users_value_is_not_found()
    {
        var owner = await CreateUserClientAsync();
        var other = await CreateUserClientAsync();
        var tracking = await CreateTrackingAsync(owner, "Steps", new Value(0, "Count", [new Metadata("unit", "steps")]));
        var valueId = tracking.Values.Single().UserTrackingValueId;

        var update = await other.PutAsJsonAsync($"/api/user-tracking-value/{valueId}",
            new { tracking.UserTrackingId, Name = "Hijacked", Type = "Number" }, Ct);
        var delete = await other.DeleteAsync($"/api/user-tracking-value/{valueId}", Ct);
        var missing = await owner.PutAsJsonAsync("/api/user-tracking-value/999999",
            new { tracking.UserTrackingId, Name = "Nope", Type = "Number" }, Ct);

        Assert.Equal(HttpStatusCode.NotFound, update.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, delete.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, missing.StatusCode);
        var value = (await GetTrackingAsync(owner, tracking.UserTrackingId)).Values.Single();
        Assert.Equal("Count", value.Name);
        Assert.Equal([new Metadata("unit", "steps")], value.Metadata);
    }

    [Fact]
    public async Task Tracking_update_prunes_values_from_other_users_and_logs()
    {
        var owner = await CreateUserClientAsync();
        var other = await CreateUserClientAsync();
        var ownersTracking = await CreateTrackingAsync(owner, "Steps", new Value(0, "Count", [new Metadata("unit", "steps")]));
        var ownersValueId = ownersTracking.Values.Single().UserTrackingValueId;
        var othersTracking = await CreateTrackingAsync(other, "Water", new Value(0, "Glasses"));
        var othersValue = othersTracking.Values.Single();

        var response = await other.PutAsJsonAsync($"/api/user-tracking/{othersTracking.UserTrackingId}", TrackingBody("Water",
            othersValue with { Name = "Cups" },
            new Value(ownersValueId, "Hijacked", [new Metadata("unit", "hacked")])), Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(["Cups"], (await GetTrackingAsync(other, othersTracking.UserTrackingId)).Values.Select(v => v.Name));
        var ownersValue = (await GetTrackingAsync(owner, ownersTracking.UserTrackingId)).Values.Single();
        Assert.Equal("Count", ownersValue.Name);
        Assert.Equal([new Metadata("unit", "steps")], ownersValue.Metadata);
        Assert.True(WarningLogged("Ignored tracking value ids", ownersValueId));
    }

    [Fact]
    public async Task Tracking_update_prunes_values_from_the_users_other_trackings()
    {
        var client = await CreateUserClientAsync();
        var steps = await CreateTrackingAsync(client, "Steps", new Value(0, "Count"));
        var water = await CreateTrackingAsync(client, "Water", new Value(0, "Glasses"));
        var stepsValueId = steps.Values.Single().UserTrackingValueId;

        var response = await client.PutAsJsonAsync($"/api/user-tracking/{water.UserTrackingId}", TrackingBody("Water",
            water.Values.Single(),
            new Value(stepsValueId, "Moved")), Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(["Count"], (await GetTrackingAsync(client, steps.UserTrackingId)).Values.Select(v => v.Name));
        Assert.Equal(["Glasses"], (await GetTrackingAsync(client, water.UserTrackingId)).Values.Select(v => v.Name));
    }

    [Fact]
    public async Task Updating_or_deleting_another_users_tracking_is_not_found()
    {
        var owner = await CreateUserClientAsync();
        var other = await CreateUserClientAsync();
        var tracking = await CreateTrackingAsync(owner, "Steps", new Value(0, "Count"));

        var update = await other.PutAsJsonAsync($"/api/user-tracking/{tracking.UserTrackingId}", TrackingBody("Hijacked"), Ct);
        var delete = await other.DeleteAsync($"/api/user-tracking/{tracking.UserTrackingId}", Ct);

        Assert.Equal(HttpStatusCode.NotFound, update.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, delete.StatusCode);
        var unchanged = await GetTrackingAsync(owner, tracking.UserTrackingId);
        Assert.Equal("Steps", unchanged.Title);
        Assert.Equal(["Count"], unchanged.Values.Select(v => v.Name));
    }

    [Fact]
    public async Task Daily_values_for_another_users_tracking_values_are_pruned_and_logged()
    {
        var owner = await CreateUserClientAsync();
        var other = await CreateUserClientAsync();
        var ownersValueId = (await CreateTrackingAsync(owner, "Steps", new Value(0, "Count"))).Values.Single().UserTrackingValueId;
        var othersValueId = (await CreateTrackingAsync(other, "Water", new Value(0, "Glasses"))).Values.Single().UserTrackingValueId;
        const string day = "2026-09-10";
        (await other.PutAsJsonAsync($"/api/day/{day}", new { Water = 0, Weight = 0 }, Ct)).EnsureSuccessStatusCode();

        var response = await other.PutAsJsonAsync($"/api/day-tracking-values/{day}", new[]
        {
            new { UserTrackingValueId = othersValueId, Occurrence = 1, Value = 8m, When = (DateTime?)null },
            new { UserTrackingValueId = ownersValueId, Occurrence = 1, Value = 99m, When = (DateTime?)null },
        }, Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var saved = await other.GetFromJsonAsync<List<JsonElement>>($"/api/day-tracking-values/{day}", Ct);
        Assert.Equal([othersValueId], saved!.Select(v => v.GetProperty("userTrackingValueId").GetInt32()));
        Assert.True(WarningLogged("Ignored tracking value ids", ownersValueId));
    }
}
