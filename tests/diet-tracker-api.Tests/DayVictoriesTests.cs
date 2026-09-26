using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;

namespace diet_tracker_api.Tests;

public class DayVictoriesTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private record Victory(int VictoryId, string? Name, DateTime? When = null, string Type = "NonScale");

    private async Task<HttpClient> CreateUserClientAsync()
    {
        var auth = await factory.RegisterAsync(factory.CreateClient(), ApiFactory.NewEmail(), "Correct-Horse-9");
        return factory.CreateAuthorizedClient(auth.AccessToken);
    }

    private static async Task<List<Victory>> SaveAsync(HttpClient client, string day, params Victory[] victories)
    {
        var response = await client.PutAsJsonAsync($"/api/day/{day}/victories", victories, Ct);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<List<Victory>>(Ct))!;
    }

    private static List<string?> Names(IEnumerable<Victory> victories) => victories.Select(v => v.Name).ToList();

    [Fact]
    public async Task Empty_day_has_no_victories()
    {
        var client = await CreateUserClientAsync();

        var victories = await client.GetFromJsonAsync<List<Victory>>("/api/day/2026-09-02/victories", Ct);

        Assert.Empty(victories!);
    }

    [Fact]
    public async Task Only_named_victories_are_saved_as_non_scale_on_that_day()
    {
        var client = await CreateUserClientAsync();
        const string day = "2026-09-03";

        var result = await SaveAsync(client, day,
            new Victory(0, "  Walked 5k  "),
            new Victory(0, null, new DateTime(2026, 9, 3, 9, 0, 0)),
            new Victory(0, "   "),
            new Victory(0, "Drank all my water", Type: "Goal"));

        Assert.Equal(["Walked 5k", "Drank all my water"], Names(result));
        Assert.All(result, v => Assert.Equal(new DateTime(2026, 9, 3), v.When));
    }

    [Fact]
    public async Task Existing_victories_are_renamed_or_removed_when_cleared()
    {
        var client = await CreateUserClientAsync();
        const string day = "2026-09-05";
        var saved = await SaveAsync(client, day, new Victory(0, "Slept 8 hours"), new Victory(0, "No snacks"));

        var result = await SaveAsync(client, day,
            saved[0] with { Name = "Slept 9 hours" },
            saved[1] with { Name = "" });

        Assert.Equal(["Slept 9 hours"], Names(result));
    }

    [Fact]
    public async Task Victories_of_other_users_are_pruned_and_logged()
    {
        var owner = await CreateUserClientAsync();
        var other = await CreateUserClientAsync();
        const string day = "2026-09-06";
        var ownersVictory = (await SaveAsync(owner, day, new Victory(0, "Ran a mile"))).Single();

        var result = await SaveAsync(other, day, new Victory(0, "Did yoga"), ownersVictory with { Name = "Hijacked" });
        await SaveAsync(other, day, ownersVictory with { Name = "" });

        Assert.Equal(["Did yoga"], Names(result));
        var ownersDay = await owner.GetFromJsonAsync<List<Victory>>($"/api/day/{day}/victories", Ct);
        Assert.Equal(["Ran a mile"], Names(ownersDay!));
        Assert.Contains(factory.Logs.Entries, log =>
            log.Level == LogLevel.Warning && log.Message.StartsWith("Ignored victory ids") && log.Message.EndsWith($"on {day}: {ownersVictory.VictoryId}"));
    }

    [Fact]
    public async Task Goal_victories_cannot_be_changed_through_the_day()
    {
        var client = await CreateUserClientAsync();
        const string day = "2026-09-07";
        var created = await client.PostAsJsonAsync("/api/victory",
            new { Name = "Lose 10 pounds", When = new DateTime(2026, 9, 7), Type = "Goal" }, Ct);
        Assert.Equal(HttpStatusCode.OK, created.StatusCode);
        var goal = await created.Content.ReadFromJsonAsync<JsonElement>(Ct);
        var goalId = goal.GetProperty("victoryId").GetInt32();

        await SaveAsync(client, day, new Victory(goalId, ""));

        var goals = await client.GetFromJsonAsync<List<Victory>>("/api/victories?type=Goal", Ct);
        Assert.Equal(["Lose 10 pounds"], Names(goals!));
    }
}
