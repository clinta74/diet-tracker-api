using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace diet_tracker_api.Tests;

public class DayFuelingsTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private const int PlanFuelingCount = 5; // ApiFactory's seeded plan
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private record Fueling(int UserFuelingId, string? Name, DateTime? When = null);

    private async Task<HttpClient> CreateUserClientAsync()
    {
        var auth = await factory.RegisterAsync(factory.CreateClient(), ApiFactory.NewEmail(), "Correct-Horse-9");
        return factory.CreateAuthorizedClient(auth.AccessToken);
    }

    private static async Task CreateDayAsync(HttpClient client, string day)
    {
        var response = await client.PutAsJsonAsync($"/api/day/{day}", new { Water = 0, Weight = 0 }, Ct);
        response.EnsureSuccessStatusCode();
    }

    private static async Task<List<Fueling>> SaveAsync(HttpClient client, string day, params Fueling[] fuelings)
    {
        var response = await client.PutAsJsonAsync($"/api/day/{day}/fuelings", fuelings, Ct);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<List<Fueling>>(Ct))!;
    }

    private static List<string?> Named(IEnumerable<Fueling> fuelings) =>
        fuelings.Where(f => f.UserFuelingId != 0).Select(f => f.Name).ToList();

    [Fact]
    public async Task Empty_day_returns_one_blank_slot_per_plan_fueling()
    {
        var client = await CreateUserClientAsync();

        var fuelings = await client.GetFromJsonAsync<List<Fueling>>("/api/day/2026-09-02/fuelings", Ct);

        Assert.Equal(PlanFuelingCount, fuelings!.Count);
        Assert.All(fuelings, f => Assert.Equal((0, ""), (f.UserFuelingId, f.Name)));
    }

    [Fact]
    public async Task Only_named_fuelings_are_saved_and_result_is_padded_to_the_plan()
    {
        var client = await CreateUserClientAsync();
        const string day = "2026-09-03";
        await CreateDayAsync(client, day);
        var when = new DateTime(2026, 9, 3, 8, 30, 0);

        var result = await SaveAsync(client, day,
            new Fueling(0, "  Bar  ", when),
            new Fueling(0, null, when),
            new Fueling(0, "   ", when),
            new Fueling(0, ""));

        Assert.Equal(["Bar"], Named(result));
        Assert.Equal(when, result.Single(f => f.UserFuelingId != 0).When);
        Assert.Equal(PlanFuelingCount, result.Count);
        Assert.Equal(PlanFuelingCount - 1, result.Count(f => f.UserFuelingId == 0 && f.Name == ""));
    }

    [Fact]
    public async Task More_fuelings_than_the_plan_are_all_returned_in_saved_order()
    {
        var client = await CreateUserClientAsync();
        const string day = "2026-09-04";
        await CreateDayAsync(client, day);
        var names = Enumerable.Range(1, PlanFuelingCount + 1).Select(i => $"Fueling {i}").ToArray();

        var result = await SaveAsync(client, day, names.Select(n => new Fueling(0, n)).ToArray());

        Assert.Equal(names, Named(result));
        Assert.Equal(PlanFuelingCount + 1, result.Count);
    }

    [Fact]
    public async Task Existing_fuelings_are_renamed_or_removed_when_cleared()
    {
        var client = await CreateUserClientAsync();
        const string day = "2026-09-05";
        await CreateDayAsync(client, day);
        var saved = (await SaveAsync(client, day, new Fueling(0, "Shake"), new Fueling(0, "Bar"))).Where(f => f.UserFuelingId != 0).ToList();

        var result = await SaveAsync(client, day,
            saved[0] with { Name = "Oatmeal" },
            saved[1] with { Name = "  " });

        Assert.Equal(["Oatmeal"], Named(result));
        Assert.Equal(PlanFuelingCount, result.Count);
    }

    [Fact]
    public async Task Fuelings_of_other_users_are_pruned_and_logged()
    {
        var owner = await CreateUserClientAsync();
        var other = await CreateUserClientAsync();
        const string day = "2026-09-06";
        await CreateDayAsync(owner, day);
        await CreateDayAsync(other, day);
        var ownersFueling = (await SaveAsync(owner, day, new Fueling(0, "Shake"))).Single(f => f.UserFuelingId != 0);
        var othersFueling = (await SaveAsync(other, day, new Fueling(0, "Bar"))).Single(f => f.UserFuelingId != 0);

        // The other user's own changes still apply; the foreign rename and clear are dropped.
        var result = await SaveAsync(other, day,
            othersFueling with { Name = "Renamed" },
            new Fueling(0, "Added"),
            ownersFueling with { Name = "Hijacked" });
        await SaveAsync(other, day, ownersFueling with { Name = "" });

        Assert.Equal(["Renamed", "Added"], Named(result));
        var ownersDay = await owner.GetFromJsonAsync<List<Fueling>>($"/api/day/{day}/fuelings", Ct);
        Assert.Equal(["Shake"], Named(ownersDay!));
        Assert.Contains(factory.Logs.Entries, log =>
            log.Level == LogLevel.Warning && log.Message.EndsWith($"on {day}: {ownersFueling.UserFuelingId}"));
    }

    [Fact]
    public async Task Own_fueling_from_another_day_is_pruned()
    {
        var client = await CreateUserClientAsync();
        await CreateDayAsync(client, "2026-09-08");
        await CreateDayAsync(client, "2026-09-09");
        var earlier = (await SaveAsync(client, "2026-09-08", new Fueling(0, "Shake"))).Single(f => f.UserFuelingId != 0);

        var later = await SaveAsync(client, "2026-09-09", earlier with { Name = "Moved" });

        Assert.Empty(Named(later));
        var earlierDay = await client.GetFromJsonAsync<List<Fueling>>("/api/day/2026-09-08/fuelings", Ct);
        Assert.Equal(["Shake"], Named(earlierDay!));
    }
}
