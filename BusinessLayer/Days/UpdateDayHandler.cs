using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.Days
{
    public record UpdateDay(DateOnly Day, string UserId, CurrentUserDay UserDay) : IRequest<Unit>;

    public class UpdateDayHandler : IRequestHandler<UpdateDay, Unit>
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
                .Where(userDay => userDay.UserId == userId && userDay.Day == day)
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            if (data == null)
            {
                _dbContext.UserDays
                    .Add(new UserDay
                    {
                        Day = day,
                        UserId = userId,
                        Water = userDay.Water,
                        Weight = userDay.Weight,
                        Notes = (userDay.Notes == null || userDay.Notes.Trim().Length == 0) ? null : userDay.Notes.Trim(),
                    });
            }
            else
            {
                _dbContext.UserDays.Update(data with
                {
                    Water = userDay.Water,
                    Weight = userDay.Weight,
                    Notes = userDay.Notes == null || userDay.Notes.Trim().Length == 0 ? null : userDay.Notes.Trim(),
                });
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            // Handle Meals
            _dbContext.UserMeals
                .AddRange(userDay.Meals
                    .Where(m => m.UserMealId == 0)
                    .Where(m => m.Name.Trim().Length > 0 || m.When != null)
                    .Select(m => m with
                    {
                        UserId = userId,
                        Day = day,
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

            var removeMealIds = userDay.Meals.Where(meal => meal.UserMealId == 0 || (string.IsNullOrWhiteSpace(meal.Name) && meal.UserMealId != 0))
                .Select(meal => meal.UserMealId);

            var removeMeals = _dbContext.UserMeals
                .Where(userMeal => userMeal.UserId == userId && userMeal.Day == day)
                .Where(userMeal => removeMealIds.Contains(userMeal.UserMealId))
                .AsEnumerable();

            _dbContext.UserMeals.RemoveRange(removeMeals);

            await _dbContext.SaveChangesAsync(cancellationToken);
            
            // Handle Fuelings
            _dbContext.UserFuelings
               .AddRange(userDay.Fuelings
                   .Where(f => f.UserFuelingId == 0)
                   .Where(f => f.Name.Trim().Length > 0 || f.When != null)
                   .Select(f => f with
                   {
                       UserId = userId,
                       Day = day,
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

            var removeFuelingIds = userDay.Fuelings.Where(fueling => fueling.UserFuelingId == 0 || (string.IsNullOrWhiteSpace(fueling.Name) && fueling.UserFuelingId != 0))
                .Select(fueling => fueling.UserFuelingId);

            var removeFuelings = _dbContext.UserFuelings
                .Where(userFueling => userFueling.UserId == userId && userFueling.Day == day)
                .Where(userFueling => removeFuelingIds.Contains(userFueling.UserFuelingId))
                .AsEnumerable();

            _dbContext.UserFuelings.RemoveRange(removeFuelings);

            await _dbContext.SaveChangesAsync(cancellationToken);

            // Handle Victories
            _dbContext.Victories
                .AddRange(userDay.Victories
                    .Where(victory => victory.VictoryId == 0)
                    .Where(victory => victory.Name.Trim().Length > 0 || victory.When != null)
                    .Select(victory => victory with
                    {
                        UserId = userId,
                        When = day.ToDateTime(TimeOnly.MinValue),
                    }));

            _dbContext.Victories
                .UpdateRange(userDay.Victories
                    .Where(victory => victory.VictoryId != 0)
                    .Select(victory => _dbContext.Victories
                        .AsNoTracking()
                        .First(v => v.VictoryId == victory.VictoryId)
                    with
                    {
                        Name = victory.Name,
                    }));

            var removeVictoryIds = userDay.Victories.Where(victory => victory.VictoryId == 0 || (string.IsNullOrWhiteSpace(victory.Name) && victory.VictoryId != 0))
                .Select(victory => victory.VictoryId);

            var removeVictories = _dbContext.Victories
                .Where(victory => victory.UserId == userId && victory.When == day.ToDateTime(TimeOnly.MinValue))
                .Where(victory => removeVictoryIds.Contains(victory.VictoryId))
                .AsEnumerable();

            _dbContext.Victories.RemoveRange(removeVictories);

            await _dbContext.SaveChangesAsync(cancellationToken);

            // All done lets commit it.
            await transaction.CommitAsync(cancellationToken);

            return Unit.Value;
        }
    }
}