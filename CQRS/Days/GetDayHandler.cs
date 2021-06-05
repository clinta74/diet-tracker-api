using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using diet_tracker_api.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.CQRS.Days
{
    public record GetDay(DateTime Date, string UserId) : IRequest<CurrentUserDay>;
    public class GetDayHandler : IRequestHandler<Days.GetDay, CurrentUserDay>
    {
        private readonly DietTrackerDbContext _dbContext;

        public GetDayHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<CurrentUserDay> Handle(Days.GetDay request, CancellationToken cancellationToken)
        {
            var data = await _dbContext.UserDays
                .Where(userDay => userDay.UserId == request.UserId)
                .Where(userDay => userDay.Day == request.Date)
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
                .AsSplitQuery()
                .SingleOrDefaultAsync(cancellationToken);

            if (data == null)
            {
                data = new CurrentUserDay
                {
                    Day = request.Date,
                    UserId = request.UserId,
                    Water = 0,
                    Weight = 0,
                    Meals = new UserMeal[0],
                    Fuelings = new UserFueling[0],
                    Condiments = 0,
                    Notes = "",
                };
            }

            var plan = await _dbContext.UserPlans
                    .OrderByDescending(up => up.Start)
                    .Where(up => up.UserId == request.UserId)
                    .Select(up => up.Plan)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(cancellationToken);

            if (plan == null)
            {
                throw ArgumentException($"User ID ({request.UserId}) has no selected plan.");
            }

            if (data.Meals.Count < plan.MealCount || data.Fuelings.Count < plan.FuelingCount)
            {
                var meals = new UserMeal[plan.MealCount - data.Meals.Count];
                Array.Fill(meals, new UserMeal { Name = "", Day = request.Date, UserId = request.UserId });

                var fuelings = new UserFueling[plan.FuelingCount - data.Fuelings.Count];
                Array.Fill(fuelings, new UserFueling { Name = "", Day = request.Date, UserId = request.UserId });

                data.Meals = data.Meals.Concat(meals).ToList();
                data.Fuelings = data.Fuelings.Concat(fuelings).ToList();
            }

            var weights = await _dbContext.UserDays
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

        private Exception ArgumentException(string v)
        {
            throw new NotImplementedException();
        }
    }
}