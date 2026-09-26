using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.Logging;

namespace diet_tracker_api.Tests;

public class DayMealsTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    // ApiFactory's seeded plan has one meal per day, so padding yields a single blank slot.
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private record Meal(int UserMealId, string? Name, DateTime? When = null);

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

    private static async Task<List<Meal>> SaveAsync(HttpClient client, string day, params Meal[] meals)
    {
        var response = await client.PutAsJsonAsync($"/api/day/{day}/meals", meals, Ct);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        return (await response.Content.ReadFromJsonAsync<List<Meal>>(Ct))!;
    }

    private static List<string?> Named(IEnumerable<Meal> meals) =>
        meals.Where(m => m.UserMealId != 0).Select(m => m.Name).ToList();

    [Fact]
    public async Task Empty_day_returns_one_blank_slot_per_plan_meal()
    {
        var client = await CreateUserClientAsync();

        var meals = await client.GetFromJsonAsync<List<Meal>>("/api/day/2026-09-02/meals", Ct);

        var slot = Assert.Single(meals!);
        Assert.Equal((0, ""), (slot.UserMealId, slot.Name));
    }

    [Fact]
    public async Task Only_named_meals_are_saved_in_order_and_result_is_at_least_the_plan()
    {
        var client = await CreateUserClientAsync();
        const string day = "2026-09-03";
        await CreateDayAsync(client, day);
        var when = new DateTime(2026, 9, 3, 18, 0, 0);

        var result = await SaveAsync(client, day,
            new Meal(0, null, when),
            new Meal(0, "  Salmon  ", when),
            new Meal(0, "   "),
            new Meal(0, "Salad"));

        Assert.Equal(["Salmon", "Salad"], Named(result));
        Assert.Equal(when, result.First(m => m.UserMealId != 0).When);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task Existing_meals_are_renamed_or_removed_when_cleared()
    {
        var client = await CreateUserClientAsync();
        const string day = "2026-09-05";
        await CreateDayAsync(client, day);
        var saved = Named(await SaveAsync(client, day, new Meal(0, "Chicken"), new Meal(0, "Steak"))).Count;
        Assert.Equal(2, saved);
        var existing = (await client.GetFromJsonAsync<List<Meal>>($"/api/day/{day}/meals", Ct))!;

        var result = await SaveAsync(client, day,
            existing[0] with { Name = "Turkey" },
            existing[1] with { Name = "" });

        Assert.Equal("Turkey", Assert.Single(result).Name);
    }

    [Fact]
    public async Task Meals_of_other_users_are_pruned_and_logged()
    {
        var owner = await CreateUserClientAsync();
        var other = await CreateUserClientAsync();
        const string day = "2026-09-06";
        await CreateDayAsync(owner, day);
        await CreateDayAsync(other, day);
        var ownersMeal = (await SaveAsync(owner, day, new Meal(0, "Chicken"))).Single(m => m.UserMealId != 0);

        var result = await SaveAsync(other, day, new Meal(0, "Steak"), ownersMeal with { Name = "Hijacked" });
        await SaveAsync(other, day, ownersMeal with { Name = "" });

        Assert.Equal(["Steak"], Named(result));
        var ownersDay = await owner.GetFromJsonAsync<List<Meal>>($"/api/day/{day}/meals", Ct);
        Assert.Equal(["Chicken"], Named(ownersDay!));
        Assert.Contains(factory.Logs.Entries, log =>
            log.Level == LogLevel.Warning && log.Message.StartsWith("Ignored meal ids") && log.Message.EndsWith($"on {day}: {ownersMeal.UserMealId}"));
    }
}
