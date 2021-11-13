using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.UserDailyTrackingValues
{
    public record GetCurrentUserDailyTrackingValues(DateOnly day, string userId) : IRequest<IEnumerable<UserDailyTrackingValue>>;
    public class GetCurrentUserDailyTrackingValuesHandler : IRequestHandler<GetCurrentUserDailyTrackingValues, IEnumerable<UserDailyTrackingValue>>
    {
        private readonly DietTrackerDbContext _dbContext;

        public GetCurrentUserDailyTrackingValuesHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
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