using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace diet_tracker_api.Tests;

/// <summary>
/// Blank fueling/meal slots follow the plan in effect on the requested day, not the current plan.
/// </summary>
public class PlanForDayTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private async Task<int> AddPlanAsync(int fuelings, int meals)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DietTrackerDbContext>();
        var plan = new Plan { Name = $"Plan {fuelings}/{meals}", FuelingCount = fuelings, MealCount = meals };
        db.Plans.Add(plan);
        await db.SaveChangesAsync(Ct);
        return plan.PlanId;
    }

    private static async Task<string> GetUserIdAsync(HttpClient client) =>
        (await client.GetFromJsonAsync<JsonElement>("/api/user", Ct)).GetProperty("userId").GetString()!;

    /// <summary>Replaces the user's plan history.</summary>
    private async Task SetPlanHistoryAsync(string userId, params (int PlanId, DateTime Start)[] plans)
    {
        using var scope = factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DietTrackerDbContext>();
        await db.UserPlans.Where(userPlan => userPlan.UserId == userId).ExecuteDeleteAsync(Ct);
        db.UserPlans.AddRange(plans.Select(p => new UserPlan
        {
            UserId = userId,
            PlanId = p.PlanId,
            Start = DateTime.SpecifyKind(p.Start, DateTimeKind.Utc),
        }));
        await db.SaveChangesAsync(Ct);
    }

    private static async Task<(int Fuelings, int Meals)> SlotsAsync(HttpClient client, string day)
    {
        var fuelings = await client.GetFromJsonAsync<List<JsonElement>>($"/api/day/{day}/fuelings", Ct);
        var meals = await client.GetFromJsonAsync<List<JsonElement>>($"/api/day/{day}/meals", Ct);
        return (fuelings!.Count, meals!.Count);
    }

    [Fact]
    public async Task Each_day_is_padded_by_the_plan_in_effect_that_day()
    {
        var client = await factory.CreateUserClientAsync();
        var userId = await GetUserIdAsync(client);
        var first = await AddPlanAsync(fuelings: 5, meals: 2);
        var second = await AddPlanAsync(fuelings: 3, meals: 1);
        await SetPlanHistoryAsync(userId, (first, new DateTime(2020, 1, 1, 14, 0, 0)), (second, new DateTime(2020, 6, 1, 14, 0, 0)));

        Assert.Equal((5, 2), await SlotsAsync(client, "2020-03-01"));
        Assert.Equal((3, 1), await SlotsAsync(client, "2020-07-01"));
        // A plan started during a day applies to that whole day.
        Assert.Equal((3, 1), await SlotsAsync(client, "2020-06-01"));
        Assert.Equal((5, 2), await SlotsAsync(client, "2020-05-31"));
    }

    [Fact]
    public async Task Days_before_the_first_plan_use_the_first_plan()
    {
        var client = await factory.CreateUserClientAsync();
        var userId = await GetUserIdAsync(client);
        var first = await AddPlanAsync(fuelings: 4, meals: 2);
        var second = await AddPlanAsync(fuelings: 6, meals: 1);
        await SetPlanHistoryAsync(userId, (first, new DateTime(2020, 1, 1)), (second, new DateTime(2020, 6, 1)));

        Assert.Equal((4, 2), await SlotsAsync(client, "2019-12-01"));
    }

    [Fact]
    public async Task Changing_plan_affects_today_but_not_earlier_days()
    {
        var client = await factory.CreateUserClientAsync();
        var newPlan = await AddPlanAsync(fuelings: 2, meals: 3);
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var earlier = DateTime.UtcNow.AddYears(-1).ToString("yyyy-MM-dd");
        var before = await SlotsAsync(client, earlier);

        var changed = await client.PutAsJsonAsync("/api/plan/change", newPlan, Ct);

        Assert.Equal(HttpStatusCode.OK, changed.StatusCode);
        Assert.Equal((2, 3), await SlotsAsync(client, today));
        Assert.Equal(before, await SlotsAsync(client, earlier));
    }
}
