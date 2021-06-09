using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.CQRS.UserDailyTrackingValues
{
    public record GetCurrentUserDailyTrackingValues(DateTime day, string userId) : IRequest<IEnumerable<UserDailyTrackingValue>>;
    public class GetCurrentUserDailyTrackingValuesHandler : IRequestHandler<GetCurrentUserDailyTrackingValues, IEnumerable<UserDailyTrackingValue>>
    {
        private readonly DietTrackerDbContext _dbContext;
        private readonly IMediator _meditor;

        public GetCurrentUserDailyTrackingValuesHandler(DietTrackerDbContext dbContext, IMediator meditor)
        {
            _dbContext = dbContext;
            _meditor = meditor;
        }

        public async Task<IEnumerable<UserDailyTrackingValue>> Handle(GetCurrentUserDailyTrackingValues request, CancellationToken cancellationToken)
        {
            var data = await _dbContext.UserDailyTrackingValues
                .AsNoTracking()
                .Where(u => u.Day.Equals(request.day))
                .Where(u => u.UserId.Equals(request.userId))
                .Select(u => new UserDailyTrackingValue
                {
                    UserId = u.UserId,
                    Day = u.Day,
                    UserTrackingValueId = u.UserTrackingValueId,
                    Occurrence = u.Occurrence,
                    Value = u.Value,
                    When = u.When,
                })
                .ToListAsync(cancellationToken);

            return data;
        }
    }
}