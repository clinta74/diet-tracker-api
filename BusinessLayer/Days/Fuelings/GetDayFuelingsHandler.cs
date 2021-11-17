using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.Days.Fuelings
{
    public record GetDayFuelings(DateTime Date, string UserId) : IRequest<IEnumerable<UserDayFueling>>;

    public record UserDayFueling(int UserFuelingId, string UserId, DateTime Day, string Name, Nullable<DateTime> When);

    public class GetDayFuelingsHandler : IRequestHandler<GetDayFuelings, IEnumerable<UserDayFueling>>
    {
        private readonly DietTrackerDbContext _dbContext;
        private readonly IMediator _mediator;

        public GetDayFuelingsHandler(DietTrackerDbContext dbContext, IMediator mediator)
        {
            _dbContext = dbContext;
            _mediator = mediator;
        }

        public async Task<IEnumerable<UserDayFueling>> Handle(GetDayFuelings request, CancellationToken cancellationToken)
        {
            var data = await _dbContext.UserFuelings
                .Where(userFueling => userFueling.UserId == request.UserId)
                .Where(userFueling => userFueling.Day == request.Date)
                .Select(userFueling => new UserDayFueling(
                    userFueling.UserFuelingId,
                    userFueling.UserId,
                    userFueling.Day,
                    userFueling.Name,
                    userFueling.When))
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var plan = await _mediator.Send(new GetCurrentUserPlan(request.UserId));

            var fuelings = new List<UserDayFueling>(data);
            if (data.Count < plan.FuelingCount)
            {
                var _fuelings = new UserDayFueling[plan.FuelingCount - data.Count];
                Array.Fill(_fuelings, new UserDayFueling(0, request.UserId, request.Date, "", null));

                fuelings.AddRange(_fuelings);
            }

            return fuelings;
        }
    }
}