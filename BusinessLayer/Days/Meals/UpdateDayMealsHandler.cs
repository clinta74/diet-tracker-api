using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.Days.Meals;

public record UpdateDayMeals(DateTime Day, string UserId, IEnumerable<UserMeal> Meals) : IRequest<Unit>;
public class UpdateDayMealsHandler(DietTrackerDbContext dbContext) : IRequestHandler<UpdateDayMeals, Unit>
{
    private readonly DietTrackerDbContext _dbContext = dbContext;

    public async ValueTask<Unit> Handle(UpdateDayMeals request, CancellationToken cancellationToken)
    {
        using var transaction = _dbContext.Database.BeginTransaction();

        // Handle Meals
        _dbContext.UserMeals
            .AddRange(request.Meals
                .Where(m => m.UserMealId == 0)
                .Where(m => !string.IsNullOrWhiteSpace(m.Name) || m.When != null)
                .Select(m => m with
                {
                    UserId = request.UserId,
                    Day = request.Day.Date,
                }));

        await _dbContext.SaveChangesAsync(cancellationToken);

        _dbContext.UserMeals
            .UpdateRange(request.Meals
                .Where(meal => meal.UserMealId != 0)
                .Select(meal => _dbContext.UserMeals
                    .AsNoTracking()
                    .First(m => m.UserMealId == meal.UserMealId)
                with
                {
                    Name = meal.Name,
                    When = meal.When,
                }));

        await _dbContext.SaveChangesAsync(cancellationToken);

        var removeMealIds = request.Meals.Where(meal => meal.UserMealId == 0 || (string.IsNullOrWhiteSpace(meal.Name) && meal.UserMealId != 0))
            .Select(meal => meal.UserMealId);

        var removeMeals = _dbContext.UserMeals
            .Where(userMeal => userMeal.UserId == request.UserId && userMeal.Day == request.Day.Date)
            .Where(userMeal => removeMealIds.Contains(userMeal.UserMealId))
            .AsEnumerable();

        _dbContext.UserMeals.RemoveRange(removeMeals);

        await _dbContext.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return Unit.Value;
    }
}