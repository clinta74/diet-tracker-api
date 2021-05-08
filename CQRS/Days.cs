using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.CQRS.Days;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using diet_tracker_api.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.CQRS
{
    namespace Days
    {
        public record GetDay(DateTime Date, string UserId) : IRequest<CurrentUserDay>;
        public record UpdateDay(DateTime Day, string UserId, UserDay UserDay) : IRequest<Unit>;
    }

    public class GetDayHandler : IRequestHandler<Days.GetDay, CurrentUserDay>
    {
        private readonly DietTrackerDbContext _dietTrackerDbContext;

        public GetDayHandler(DietTrackerDbContext dietTrackerDbContext)
        {
            _dietTrackerDbContext = dietTrackerDbContext;
        }

        public async Task<CurrentUserDay> Handle(Days.GetDay request, CancellationToken cancellationToken)
        {
            var data = await _dietTrackerDbContext.UserDays
                .Where(userDay => userDay.UserId == request.UserId && userDay.Day == request.Date)
                .Include(userDay => userDay.Fuelings)
                .Include(userDay => userDay.Meals)
                .Select(userDay => new CurrentUserDay
                {
                    Day = userDay.Day,
                    UserId = userDay.UserId,
                    Water = userDay.Water,
                    Weight = userDay.Weight,
                    Meals = userDay.Meals,
                    Fuelings = userDay.Fuelings,
                    Condiments = userDay.Condiments,
                    Notes = userDay.Notes,
                })
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            if (data == null)
            {
                var plan = await _dietTrackerDbContext.UserPlans
                    .OrderByDescending(up => up.Start)
                    .Where(up => up.UserId == request.UserId)
                    .Select(up => up.Plan)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(cancellationToken);

                var meals = new UserMeal[plan.MealCount];
                Array.Fill(meals, new UserMeal { Name = "", Day = request.Date, UserId = request.UserId });

                var fuelings = new UserFueling[plan.FuelingCount];
                Array.Fill(fuelings, new UserFueling { Name = "", Day = request.Date, UserId = request.UserId });

                data = new CurrentUserDay
                {
                    Day = request.Date,
                    UserId = request.UserId,
                    Water = 0,
                    Weight = 0,
                    Meals = meals,
                    Fuelings = fuelings,
                    Condiments = 0,
                    Notes = "",
                };
            }

            var weights = await _dietTrackerDbContext.UserDays
                .Where(userDay => userDay.UserId == request.UserId && userDay.Weight > 0)
                .OrderBy(userDay => userDay.Day)
                .Where(userDay => userDay.Day <= request.Date)
                .AsNoTracking()
                .Select(userDay => userDay.Weight)
                .ToListAsync(cancellationToken);

            if (weights.Count > 1)
            {
                return data with
                {
                    CumulativeWeightChange = weights.First() - weights.Last(),
                    WeightChange = weights[weights.Count - 2] - weights.Last(),
                };
            }

            if (weights.Count > 0)
            {
                return data with
                {
                    CumulativeWeightChange = weights.First() - weights.Last(),
                };
            }

            return data;
        }
    }

    public class UpdateDayHandler : IRequestHandler<Days.UpdateDay, Unit>
    {
        private readonly DietTrackerDbContext _dietTrackerDbContext;
        public UpdateDayHandler(DietTrackerDbContext dietTrackerDbContext)
        {
            _dietTrackerDbContext = dietTrackerDbContext;
        }
        public async Task<Unit> Handle(UpdateDay request, CancellationToken cancellationToken)
        {
            using var transaction = _dietTrackerDbContext.Database.BeginTransaction();

            var userId = request.UserId;
            var day = request.Day;
            var userDay = request.UserDay;

            var data = await _dietTrackerDbContext.UserDays
                .Where(userDay => userDay.UserId == userId && userDay.Day == day.Date)
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            if (data == null)
            {
                _dietTrackerDbContext.UserDays
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
                _dietTrackerDbContext.UserDays.Update(data with
                {
                    Water = userDay.Water,
                    Weight = userDay.Weight,
                    Condiments = userDay.Condiments,
                    Notes = userDay.Notes,
                });
            }

            await _dietTrackerDbContext.SaveChangesAsync(cancellationToken);

            _dietTrackerDbContext.UserMeals
                .AddRange(userDay.Meals
                    .Where(m => m.UserMealId == 0)
                    .Select(m => m with
                    {
                        UserId = userId,
                        Day = day.Date,
                    }));


            _dietTrackerDbContext.UserFuelings
                .AddRange(userDay.Fuelings
                    .Where(f => f.UserFuelingId == 0)
                    .Select(f => f with
                    {
                        UserId = userId,
                        Day = day.Date,
                    }));

            _dietTrackerDbContext.UserMeals
                .UpdateRange(userDay.Meals
                    .Where(meal => meal.UserMealId != 0)
                    .Select(meal => _dietTrackerDbContext.UserMeals
                        .AsNoTracking()
                        .First(m => m.UserMealId == meal.UserMealId)
                    with
                    {
                        Name = meal.Name,
                        When = meal.When,
                    }));

            _dietTrackerDbContext.UserFuelings
                .UpdateRange(userDay.Fuelings
                    .Where(fueling => fueling.UserFuelingId != 0)
                    .Select(fueling => _dietTrackerDbContext.UserFuelings
                        .AsNoTracking()
                        .First(f => f.UserFuelingId == fueling.UserFuelingId)
                    with
                    {
                        Name = fueling.Name,
                        When = fueling.When,
                    }));

            // Clean up left over entries not being used.
            var removeMealIds = userDay.Meals.Where(m => m.UserMealId != 0).Select(m => m.UserMealId);

            var removeMeals = _dietTrackerDbContext.UserMeals
                    .Where(userMeal => userMeal.UserId == userId && userMeal.Day == day.Date)
                    .Where(userMeal => !removeMealIds.Contains(userMeal.UserMealId))
                    .AsEnumerable();

            _dietTrackerDbContext.UserMeals.RemoveRange(removeMeals);

            var removeFuelingIds = userDay.Fuelings.Where(fueling => fueling.UserFuelingId != 0).Select(fueling => fueling.UserFuelingId);
            var removeFuelings = _dietTrackerDbContext.UserFuelings
                    .Where(userFueling => userFueling.UserId == userId && userFueling.Day == day.Date)
                    .Where(userFueling => !removeFuelingIds.Contains(userFueling.UserFuelingId))
                    .AsEnumerable();

            _dietTrackerDbContext.UserFuelings.RemoveRange(removeFuelings);

            await _dietTrackerDbContext.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return Unit.Value;
        }
    }
}