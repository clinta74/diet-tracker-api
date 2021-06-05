using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.CQRS.Days
{
    public record UpdateDay(DateTime Day, string UserId, UserDay UserDay) : IRequest<Unit>;
    
    public class UpdateDayHandler : IRequestHandler<Days.UpdateDay, Unit>
    {
        private readonly DietTrackerDbContext _dbContext;
        public UpdateDayHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<Unit> Handle(UpdateDay request, CancellationToken cancellationToken)
        {
            using var transaction = _dbContext.Database.BeginTransaction();

            var userId = request.UserId;
            var day = request.Day;
            var userDay = request.UserDay;

            var data = await _dbContext.UserDays
                .Where(userDay => userDay.UserId == userId && userDay.Day == day.Date)
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            if (data == null)
            {
                _dbContext.UserDays
                    .Add(new UserDay
                    {
                        Day = day.Date,
                        UserId = userId,
                        Water = userDay.Water,
                        Weight = userDay.Weight,
                        Condiments = userDay.Condiments,
                        Notes = userDay.Notes,
                    });
            }
            else
            {
                _dbContext.UserDays.Update(data with
                {
                    Water = userDay.Water,
                    Weight = userDay.Weight,
                    Condiments = userDay.Condiments,
                    Notes = userDay.Notes,
                });
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            _dbContext.UserMeals
                .AddRange(userDay.Meals
                    .Where(m => m.UserMealId == 0)
                    .Where(m => m.Name.Trim().Length > 0 || m.When != null)
                    .Select(m => m with
                    {
                        UserId = userId,
                        Day = day.Date,
                    }));


            _dbContext.UserFuelings
                .AddRange(userDay.Fuelings
                    .Where(f => f.UserFuelingId == 0)
                    .Where(f => f.Name.Trim().Length > 0 || f.When != null)
                    .Select(f => f with
                    {
                        UserId = userId,
                        Day = day.Date,
                    }));

            _dbContext.UserMeals
                .UpdateRange(userDay.Meals
                    .Where(meal => meal.UserMealId != 0)
                    .Select(meal => _dbContext.UserMeals
                        .AsNoTracking()
                        .First(m => m.UserMealId == meal.UserMealId)
                    with
                    {
                        Name = meal.Name,
                        When = meal.When,
                    }));

            _dbContext.UserFuelings
                .UpdateRange(userDay.Fuelings
                    .Where(fueling => fueling.UserFuelingId != 0)
                    .Select(fueling => _dbContext.UserFuelings
                        .AsNoTracking()
                        .First(f => f.UserFuelingId == fueling.UserFuelingId)
                    with
                    {
                        Name = fueling.Name,
                        When = fueling.When,
                    }));

            // Clean up left over entries not being used.
            var removeMealIds = userDay.Meals.Where(m => m.UserMealId != 0).Select(m => m.UserMealId);

            var removeMeals = _dbContext.UserMeals
                    .Where(userMeal => userMeal.UserId == userId && userMeal.Day == day.Date)
                    .Where(userMeal => !removeMealIds.Contains(userMeal.UserMealId))
                    .AsEnumerable();

            _dbContext.UserMeals.RemoveRange(removeMeals);

            var removeFuelingIds = userDay.Fuelings.Where(fueling => fueling.UserFuelingId != 0).Select(fueling => fueling.UserFuelingId);
            var removeFuelings = _dbContext.UserFuelings
                    .Where(userFueling => userFueling.UserId == userId && userFueling.Day == day.Date)
                    .Where(userFueling => !removeFuelingIds.Contains(userFueling.UserFuelingId))
                    .AsEnumerable();

            _dbContext.UserFuelings.RemoveRange(removeFuelings);

            await _dbContext.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return Unit.Value;
        }
    }
}