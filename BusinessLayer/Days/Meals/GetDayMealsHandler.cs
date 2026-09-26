using diet_tracker_api.DataLayer;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.Days.Meals;

public record GetDayMeals(DateTime Date, string UserId) : IRequest<IEnumerable<UserDayMeal>>;

public record UserDayMeal(int UserMealId, string? UserId, DateTime Day, string? Name, DateTime? When);

public class GetDayMealsHandler(DietTrackerDbContext dbContext, IMediator mediator) : IRequestHandler<GetDayMeals, IEnumerable<UserDayMeal>>
{
    private readonly DietTrackerDbContext _dbContext = dbContext;
    private readonly IMediator _mediator = mediator;

    public async ValueTask<IEnumerable<UserDayMeal>> Handle(GetDayMeals request, CancellationToken cancellationToken)
    {
        var data = await _dbContext.UserMeals
            .Where(userMeal => userMeal.UserId == request.UserId)
            .Where(userMeals => userMeals.Day == request.Date)
            .OrderBy(userMeals => userMeals.UserMealId)
            .Select(userMeals => new UserDayMeal(
                userMeals.UserMealId,
                userMeals.UserId,
                userMeals.Day,
                userMeals.Name,
                userMeals.When))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var plan = await _mediator.Send(new GetCurrentUserPlan(request.UserId), cancellationToken);

        // Always return at least the plan's meals per day; unused slots are blank placeholders.
        var placeholders = Enumerable.Repeat(
            new UserDayMeal(0, request.UserId, request.Date, "", null),
            Math.Max(0, plan.MealCount - data.Count));

        return [.. data, .. placeholders];
    }
}