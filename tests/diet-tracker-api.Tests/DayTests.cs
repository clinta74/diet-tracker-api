using System.Net;
using System.Net.Http.Json;

namespace diet_tracker_api.Tests;

/// <summary>
/// GET/PUT /api/day/{day} and the weight/water graphs.
/// </summary>
public class DayTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private record UserDay(DateTime Day, int Water, decimal Weight, string? Notes, decimal CumulativeWeightChange, decimal WeightChange);
    private record GraphValue(decimal Value, DateTime Date);

    private static async Task<UserDay> SaveAsync(HttpClient client, string day, int water = 0, decimal weight = 0, string? notes = null)
    {
        var response = await client.PutAsJsonAsync($"/api/day/{day}", new { Water = water, Weight = weight, Notes = notes }, Ct);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<UserDay>(Ct))!;
    }

    private static async Task<UserDay> GetAsync(HttpClient client, string day) =>
        (await client.GetFromJsonAsync<UserDay>($"/api/day/{day}", Ct))!;

    [Fact]
    public async Task Unsaved_day_returns_defaults()
    {
        var client = await factory.CreateUserClientAsync();

        var day = await GetAsync(client, "2026-09-01");

        Assert.Equal(new UserDay(new DateTime(2026, 9, 1), 0, 0, null, 0, 0), day);
    }

    [Fact]
    public async Task Saving_creates_then_updates_the_day()
    {
        var client = await factory.CreateUserClientAsync();

        var created = await SaveAsync(client, "2026-09-02", water: 3, weight: 200.5m, notes: "  Felt good  ");
        var updated = await SaveAsync(client, "2026-09-02", water: 5, weight: 199m, notes: "   ");

        Assert.Equal((3, 200.5m, "Felt good"), (created.Water, created.Weight, created.Notes));
        Assert.Equal((5, 199m, (string?)null), (updated.Water, updated.Weight, updated.Notes));
        Assert.Equal(updated, await GetAsync(client, "2026-09-02"));
    }

    [Fact]
    public async Task Weight_changes_use_weighed_days_up_to_the_day()
    {
        var client = await factory.CreateUserClientAsync();
        await SaveAsync(client, "2026-08-01", weight: 210m);
        await SaveAsync(client, "2026-08-02", water: 4);          // not weighed, ignored
        await SaveAsync(client, "2026-08-03", weight: 206m);
        await SaveAsync(client, "2026-08-05", weight: 205m);
        await SaveAsync(client, "2026-08-09", weight: 190m);      // after the day, ignored

        var firstDay = await GetAsync(client, "2026-08-01");
        var day = await GetAsync(client, "2026-08-05");
        var unweighed = await GetAsync(client, "2026-08-04");

        Assert.Equal((0m, 0m), (firstDay.CumulativeWeightChange, firstDay.WeightChange));
        Assert.Equal((5m, 1m), (day.CumulativeWeightChange, day.WeightChange));
        Assert.Equal((4m, 4m), (unweighed.CumulativeWeightChange, unweighed.WeightChange));
    }

    [Fact]
    public async Task Days_are_private_to_each_user()
    {
        var owner = await factory.CreateUserClientAsync();
        var other = await factory.CreateUserClientAsync();
        await SaveAsync(owner, "2026-09-03", water: 8, weight: 180m, notes: "Mine");

        var othersView = await GetAsync(other, "2026-09-03");

        Assert.Equal((0, 0m, (string?)null), (othersView.Water, othersView.Weight, othersView.Notes));
    }

    [Fact]
    public async Task Graphs_return_recorded_days_in_date_order_within_the_range()
    {
        var client = await factory.CreateUserClientAsync();
        var other = await factory.CreateUserClientAsync();
        // Saved out of order so the result order comes from the query, not insertion.
        await SaveAsync(client, "2026-07-10", water: 6, weight: 190m);
        await SaveAsync(client, "2026-07-01", water: 8, weight: 195m);
        await SaveAsync(client, "2026-07-05", water: 0, weight: 0m);  // nothing recorded
        await SaveAsync(client, "2026-07-03", water: 7, weight: 0m);
        await SaveAsync(client, "2026-06-20", water: 5, weight: 199m); // before the range
        await SaveAsync(other, "2026-07-02", water: 9, weight: 150m);

        var weight = await client.GetFromJsonAsync<List<GraphValue>>("/api/day/weight?startDate=2026-07-01", Ct);
        var water = await client.GetFromJsonAsync<List<GraphValue>>("/api/day/water?startDate=2026-07-01&endDate=2026-07-05", Ct);

        Assert.Equal([new(195m, new DateTime(2026, 7, 1)), new(190m, new DateTime(2026, 7, 10))], weight);
        Assert.Equal([new(8m, new DateTime(2026, 7, 1)), new(7m, new DateTime(2026, 7, 3))], water);
    }
}
