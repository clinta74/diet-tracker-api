using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.Days.Meals;

public record UpdateDayMeals(DateTime Day, string UserId, IEnumerable<UserMeal> Meals) : IRequest<Unit>;
public class UpdateDayMealsHandler(DietTrackerDbContext dbContext, ILogger<UpdateDayMealsHandler> logger) : IRequestHandler<UpdateDayMeals, Unit>
{
    private readonly DietTrackerDbContext _dbContext = dbContext;
    private readonly ILogger<UpdateDayMealsHandler> _logger = logger;

    public async ValueTask<Unit> Handle(UpdateDayMeals request, CancellationToken cancellationToken)
    {
        var day = request.Day.Date;

        // Only the user's own meals for this day can be changed; any other ids are pruned.
        var existing = await _dbContext.UserMeals
            .Where(meal => meal.UserId == request.UserId && meal.Day == day)
            .ToDictionaryAsync(meal => meal.UserMealId, cancellationToken);

        var ignoredIds = request.Meals
            .Select(meal => meal.UserMealId)
            .Where(id => id != 0 && !existing.ContainsKey(id))
            .Distinct()
            .ToList();

        if (ignoredIds.Count > 0)
        {
            _logger.LogWarning("Ignored meal ids not belonging to user {UserId} on {Day:yyyy-MM-dd}: {MealIds}",
                request.UserId, day, ignoredIds);
        }

        foreach (var meal in request.Meals.Where(meal => meal.UserMealId == 0 || existing.ContainsKey(meal.UserMealId)))
        {
            // A meal is only kept when it has a name; clearing the name removes it.
            var name = meal.Name?.Trim();
            var hasName = !string.IsNullOrEmpty(name);

            if (meal.UserMealId == 0)
            {
                if (hasName)
                {
                    _dbContext.UserMeals.Add(new UserMeal
                    {
                        UserId = request.UserId,
                        Day = day,
                        Name = name,
                        When = meal.When,
                    });
                }
            }
            else
            {
                var current = existing[meal.UserMealId];

                if (hasName)
                {
                    _dbContext.Entry(current).CurrentValues.SetValues(current with { Name = name, When = meal.When });
                }
                else
                {
                    _dbContext.UserMeals.Remove(current);
                }
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
