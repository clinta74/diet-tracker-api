using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using diet_tracker_api.DataLayer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace diet_tracker_api.Tests;

/// <summary>
/// Custom tracking items: trackings, their values and metadata, and the daily values recorded against them.
/// </summary>
public class TrackingTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private record Metadata(string Key, string? Value);
    private record Value(int UserTrackingValueId, string? Name, int Order = 0, bool Disabled = false, string Type = "Number", Metadata[]? Metadata = null);
    private record Tracking(int UserTrackingId, string? Title, int Occurrences, int Order, bool Disabled, bool UseTime, List<Value> Values);
    private record DailyValue(int UserTrackingValueId, int Occurrence, decimal Value, DateTime? When, DateTime Day);

    private static object TrackingBody(string title, int occurrences = 1, bool disabled = false, bool useTime = false, params Value[] values) => new
    {
        Title = title,
        Description = $"{title} description",
        Occurrences = occurrences,
        Order = 0,
        Disabled = disabled,
        UseTime = useTime,
        Values = values,
    };

    private static async Task<Tracking> CreateAsync(HttpClient client, string title, int occurrences = 1, bool useTime = false, params Value[] values)
    {
        var response = await client.PostAsJsonAsync("/api/user-tracking", TrackingBody(title, occurrences, useTime: useTime, values: values), Ct);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<Tracking>(Ct))!;
    }

    private static async Task<Tracking> GetAsync(HttpClient client, int userTrackingId) =>
        (await client.GetFromJsonAsync<Tracking>($"/api/user-tracking/{userTrackingId}", Ct))!;

    private static async Task<List<DailyValue>> SaveDailyAsync(HttpClient client, string day, params DailyValue[] values)
    {
        var response = await client.PutAsJsonAsync($"/api/day-tracking-values/{day}", values, Ct);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<List<DailyValue>>(Ct))!;
    }

    private static async Task CreateDayAsync(HttpClient client, string day) =>
        (await client.PutAsJsonAsync($"/api/day/{day}", new { Water = 0, Weight = 0 }, Ct)).EnsureSuccessStatusCode();

    [Fact]
    public async Task New_trackings_are_ordered_after_existing_ones_and_listed_in_order()
    {
        var client = await factory.CreateUserClientAsync();
        var water = await CreateAsync(client, "Water");
        var steps = await CreateAsync(client, "Steps", useTime: true);
        var sleep = await CreateAsync(client, "Sleep");

        var all = await client.GetFromJsonAsync<List<Tracking>>("/api/user-trackings", Ct);

        Assert.Equal([water.Order + 1, water.Order + 2], new[] { steps.Order, sleep.Order });
        Assert.Equal(["Water", "Steps", "Sleep"], all!.Select(t => t.Title));
        Assert.True(all!.Single(t => t.Title == "Steps").UseTime);
    }

    [Fact]
    public async Task Values_are_returned_in_order_with_their_metadata()
    {
        var client = await factory.CreateUserClientAsync();

        var tracking = await CreateAsync(client, "Mood", values:
        [
            new Value(0, "Evening", Order: 2),
            new Value(0, "Morning", Order: 1, Type: "Icon", Metadata: [new Metadata("icon", "sun")]),
        ]);

        var fetched = await GetAsync(client, tracking.UserTrackingId);
        Assert.Equal(["Morning", "Evening"], fetched.Values.Select(v => v.Name));
        Assert.Equal("Icon", fetched.Values[0].Type);
        Assert.Equal([new Metadata("icon", "sun")], fetched.Values[0].Metadata ?? []);
    }

    [Fact]
    public async Task Active_trackings_exclude_disabled_trackings_and_values()
    {
        var client = await factory.CreateUserClientAsync();
        var kept = await CreateAsync(client, "Water", values: [new Value(0, "Glasses"), new Value(0, "Old", Order: 1)]);
        var hidden = await CreateAsync(client, "Retired", values: [new Value(0, "Count")]);
        var oldValue = kept.Values.Single(v => v.Name == "Old");

        (await client.PutAsJsonAsync($"/api/user-tracking/{kept.UserTrackingId}",
            TrackingBody("Water", values: [kept.Values.Single(v => v.Name == "Glasses"), oldValue with { Disabled = true }]), Ct)).EnsureSuccessStatusCode();
        (await client.PutAsJsonAsync($"/api/user-tracking/{hidden.UserTrackingId}",
            TrackingBody("Retired", disabled: true, values: [.. hidden.Values]), Ct)).EnsureSuccessStatusCode();

        var active = await client.GetFromJsonAsync<List<Tracking>>("/api/user-trackings/active", Ct);
        var all = await client.GetFromJsonAsync<List<Tracking>>("/api/user-trackings", Ct);

        var water = Assert.Single(active!);
        Assert.Equal(["Glasses"], water.Values.Select(v => v.Name));
        Assert.Equal(["Water", "Retired"], all!.Select(t => t.Title));
    }

    [Fact]
    public async Task Updating_a_tracking_adds_updates_and_removes_values()
    {
        var client = await factory.CreateUserClientAsync();
        var tracking = await CreateAsync(client, "Water", values:
        [
            new Value(0, "Glasses", Metadata: [new Metadata("unit", "8oz")]),
            new Value(0, "Bottles", Order: 1),
        ]);
        var glasses = tracking.Values.Single(v => v.Name == "Glasses");

        var response = await client.PutAsJsonAsync($"/api/user-tracking/{tracking.UserTrackingId}", TrackingBody("Hydration", occurrences: 3, useTime: true, values:
        [
            glasses with { Name = "Cups", Metadata = [new Metadata("unit", "12oz"), new Metadata("goal", "8")] },
            new Value(0, "Tea", Order: 1),
        ]), Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await GetAsync(client, tracking.UserTrackingId);
        Assert.Equal(("Hydration", 3, true), (updated.Title, updated.Occurrences, updated.UseTime));
        Assert.Equal(["Cups", "Tea"], updated.Values.Select(v => v.Name));
        Assert.Equal(glasses.UserTrackingValueId, updated.Values[0].UserTrackingValueId);
        Assert.Equal([new Metadata("goal", "8"), new Metadata("unit", "12oz")], (updated.Values[0].Metadata ?? []).OrderBy(m => m.Key));
    }

    [Fact]
    public async Task Removing_a_value_with_recorded_daily_values_succeeds()
    {
        var client = await factory.CreateUserClientAsync();
        var tracking = await CreateAsync(client, "Water", values: [new Value(0, "Glasses"), new Value(0, "Bottles", Order: 1)]);
        var bottles = tracking.Values.Single(v => v.Name == "Bottles");
        await CreateDayAsync(client, "2026-09-04");
        await SaveDailyAsync(client, "2026-09-04", new DailyValue(bottles.UserTrackingValueId, 1, 2m, null, default));

        var response = await client.PutAsJsonAsync($"/api/user-tracking/{tracking.UserTrackingId}",
            TrackingBody("Water", values: [tracking.Values.Single(v => v.Name == "Glasses")]), Ct);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(["Glasses"], (await GetAsync(client, tracking.UserTrackingId)).Values.Select(v => v.Name));
    }

    [Fact]
    public async Task Deleting_a_tracking_removes_its_values_metadata_and_daily_values()
    {
        var client = await factory.CreateUserClientAsync();
        var tracking = await CreateAsync(client, "Steps", values: [new Value(0, "Count", Metadata: [new Metadata("unit", "steps")])]);
        var valueId = tracking.Values.Single().UserTrackingValueId;
        await CreateDayAsync(client, "2026-09-01");
        await SaveDailyAsync(client, "2026-09-01", new DailyValue(valueId, 1, 5000m, null, default));

        var deleted = await client.DeleteAsync($"/api/user-tracking/{tracking.UserTrackingId}", Ct);

        Assert.Equal(HttpStatusCode.OK, deleted.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await client.GetAsync($"/api/user-tracking/{tracking.UserTrackingId}", Ct)).StatusCode);
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DietTrackerDbContext>();
        Assert.False(await db.UserTrackingValues.AnyAsync(v => v.UserTrackingValueId == valueId, Ct));
        Assert.False(await db.UserTrackingValueMetadata.AnyAsync(m => m.UserTrackingValueId == valueId, Ct));
        Assert.False(await db.UserDailyTrackingValues.AnyAsync(d => d.UserTrackingValueId == valueId, Ct));
    }

    [Fact]
    public async Task Daily_values_are_saved_per_occurrence_and_only_recorded_ones_are_returned()
    {
        var client = await factory.CreateUserClientAsync();
        var tracking = await CreateAsync(client, "Water", occurrences: 3, useTime: true, values: [new Value(0, "Glasses")]);
        var valueId = tracking.Values.Single().UserTrackingValueId;
        const string day = "2026-09-02";
        await CreateDayAsync(client, day);
        var morning = new DateTime(2026, 9, 2, 8, 0, 0);

        await SaveDailyAsync(client, day,
            new DailyValue(valueId, 1, 2m, morning, default),
            new DailyValue(valueId, 3, 1m, null, default));
        var updated = await SaveDailyAsync(client, day, new DailyValue(valueId, 1, 4m, morning.AddHours(1), default));

        // The UI renders blanks for occurrences with no value; the API returns only what was recorded.
        var values = await client.GetFromJsonAsync<List<DailyValue>>($"/api/day-tracking-values/{day}", Ct);
        Assert.Equal([(1, 4m, (DateTime?)morning.AddHours(1)), (3, 1m, null)],
            values!.OrderBy(v => v.Occurrence).Select(v => (v.Occurrence, v.Value, v.When)));
        Assert.Equal(4m, Assert.Single(updated).Value);
    }

    [Fact]
    public async Task Daily_values_outside_the_trackings_occurrences_are_pruned_and_logged()
    {
        var client = await factory.CreateUserClientAsync();
        var valueId = (await CreateAsync(client, "Water", occurrences: 2, values: [new Value(0, "Glasses")])).Values.Single().UserTrackingValueId;
        const string day = "2026-09-03";
        await CreateDayAsync(client, day);

        var saved = await SaveDailyAsync(client, day,
            new DailyValue(valueId, 0, 1m, null, default),
            new DailyValue(valueId, 2, 2m, null, default),
            new DailyValue(valueId, 3, 3m, null, default));

        Assert.Equal([2], saved.Select(v => v.Occurrence));
        Assert.Contains(factory.Logs.Entries, log => log.Level == LogLevel.Warning
            && log.Message.StartsWith("Ignored out-of-range tracking value occurrences")
            && log.Message.EndsWith($"{valueId}#0, {valueId}#3"));
    }

    [Fact]
    public async Task History_returns_the_trackings_daily_values_in_day_order_within_the_range()
    {
        var client = await factory.CreateUserClientAsync();
        var water = await CreateAsync(client, "Water", occurrences: 2, values: [new Value(0, "Glasses")]);
        var steps = await CreateAsync(client, "Steps", values: [new Value(0, "Count")]);
        var waterId = water.Values.Single().UserTrackingValueId;
        var stepsId = steps.Values.Single().UserTrackingValueId;
        foreach (var day in new[] { "2026-08-03", "2026-08-01", "2026-08-02", "2026-07-31" })
        {
            await CreateDayAsync(client, day);
        }
        await SaveDailyAsync(client, "2026-08-03", new DailyValue(waterId, 2, 6m, null, default), new DailyValue(waterId, 1, 5m, null, default));
        await SaveDailyAsync(client, "2026-08-01", new DailyValue(waterId, 1, 3m, null, default), new DailyValue(stepsId, 1, 9000m, null, default));
        await SaveDailyAsync(client, "2026-08-02", new DailyValue(waterId, 1, 4m, null, default));
        await SaveDailyAsync(client, "2026-07-31", new DailyValue(waterId, 1, 1m, null, default));

        var history = await client.GetFromJsonAsync<List<DailyValue>>(
            $"/api/day-tracking-values/{water.UserTrackingId}/history?startDate=2026-08-01&endDate=2026-08-03", Ct);

        Assert.Equal([3m, 4m, 5m, 6m], history!.Select(v => v.Value));
        Assert.All(history!, v => Assert.Equal(waterId, v.UserTrackingValueId));
    }
}
