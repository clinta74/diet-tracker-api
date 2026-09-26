using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace diet_tracker_api.Tests;

public class PlanTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private const int MissingPlanId = 999_999;
    private static CancellationToken Ct => TestContext.Current.CancellationToken;

    private async Task<HttpClient> CreateUserClientAsync()
    {
        var auth = await factory.RegisterAsync(factory.CreateClient(), ApiFactory.NewEmail(), "Correct-Horse-9");
        return factory.CreateAuthorizedClient(auth.AccessToken);
    }

    [Fact]
    public async Task Plans_can_be_created_read_updated_and_deleted()
    {
        var client = await CreateUserClientAsync();

        var created = await client.PostAsJsonAsync("/api/plan", new { Name = "Crud Plan", FuelingCount = 4, MealCount = 2 }, Ct);
        Assert.Equal(HttpStatusCode.OK, created.StatusCode);
        var planId = await created.Content.ReadFromJsonAsync<int>(Ct);

        var fetched = await client.GetFromJsonAsync<JsonElement>($"/api/plan/{planId}", Ct);
        Assert.Equal("Crud Plan", fetched.GetProperty("name").GetString());

        var updated = await client.PutAsJsonAsync($"/api/plan/{planId}", new { Name = "Renamed", FuelingCount = 5, MealCount = 1 }, Ct);
        Assert.Equal(HttpStatusCode.OK, updated.StatusCode);
        var updatedPlan = await updated.Content.ReadFromJsonAsync<JsonElement>(Ct);
        Assert.Equal("Renamed", updatedPlan.GetProperty("name").GetString());
        Assert.Equal(5, updatedPlan.GetProperty("fuelingCount").GetInt32());

        var deleted = await client.DeleteAsync($"/api/plan/{planId}", Ct);
        Assert.Equal(HttpStatusCode.OK, deleted.StatusCode);

        var afterDelete = await client.GetAsync($"/api/plan/{planId}", Ct);
        Assert.Equal(HttpStatusCode.NotFound, afterDelete.StatusCode);
    }

    [Fact]
    public async Task Missing_plans_return_not_found()
    {
        var client = await CreateUserClientAsync();

        var get = await client.GetAsync($"/api/plan/{MissingPlanId}", Ct);
        var update = await client.PutAsJsonAsync($"/api/plan/{MissingPlanId}", new { Name = "x", FuelingCount = 1, MealCount = 1 }, Ct);
        var delete = await client.DeleteAsync($"/api/plan/{MissingPlanId}", Ct);

        Assert.Equal(HttpStatusCode.NotFound, get.StatusCode);
        Assert.Equal($"Plan Id ({MissingPlanId}) not found.", await get.Content.ReadFromJsonAsync<string>(Ct));
        Assert.Equal(HttpStatusCode.NotFound, update.StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, delete.StatusCode);
    }
}
