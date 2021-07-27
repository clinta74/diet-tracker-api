using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.CQRS.Victories;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.CQRS.Days
{
    public record CurrentUserDay : UserDay
    {
        public decimal CumulativeWeightChange { get; init; }
        public decimal WeightChange { get; init; }
        public IEnumerable<Victory> Victories { get; init; }
    }

    public record GetDay(DateTime Date, string UserId) : IRequest<CurrentUserDay>;
    public class GetDayHandler : IRequestHandler<Days.GetDay, CurrentUserDay>
    {
        private readonly DietTrackerDbContext _dbContext;
        private readonly IMediator _mediator;

        public GetDayHandler(DietTrackerDbContext dbContext, IMediator mediator)
        {
            _dbContext = dbContext;
            _mediator = mediator;
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
                    Notes = null,
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
                throw new ArgumentException($"User ID ({request.UserId}) has no selected plan.");
            }

            var meals = new List<UserMeal>(data.Meals);
            if (data.Meals.Count < plan.MealCount)
            {
                var _meals = new UserMeal[plan.MealCount - data.Meals.Count];
                Array.Fill(_meals, new UserMeal { Name = "", Day = request.Date, UserId = request.UserId });

                meals.AddRange(_meals);
            }

            var fuelings = new List<UserFueling>(data.Fuelings);
            if (data.Fuelings.Count < plan.FuelingCount)
            {
                var _fuelings = new UserFueling[plan.FuelingCount - data.Fuelings.Count];
                Array.Fill(_fuelings, new UserFueling { Name = "", Day = request.Date, UserId = request.UserId });

                fuelings.AddRange(_fuelings);
            }

            var victories = await _mediator.Send(new GetVictories(request.UserId, VictoryType.NonScale, request.Date));

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
                    Meals = meals,
                    Fuelings = fuelings,
                    Victories = victories,
                    CumulativeWeightChange = weights.First() - weights.Last(),
                    WeightChange = weights[weights.Count - 2] - weights.Last(),
                };
            }

            if (weights.Count > 0)
            {
                return data with
                {
                    Meals = meals,
                    Fuelings = fuelings,
                    Victories = victories,
                    CumulativeWeightChange = weights.First() - weights.Last(),
                };
            }

            return data;
        }
    }
}