using System.Collections.Generic;
using System.Linq;
using System.Threading;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.UserDailyTrackingValues
{
    public record GetCurrentUserDailyTrackingValuesHistory(string userId, int UserTrackingId, DateTime? StartDate, DateTime? EndDate) :
        IStreamRequest<UserDailyTrackingValue>;
    public class GetCurrentUserDailyTrackingValuesHistoryHandler : IStreamRequestHandler<GetCurrentUserDailyTrackingValuesHistory, UserDailyTrackingValue>
    {
        private readonly DietTrackerDbContext _dbContext;

        public GetCurrentUserDailyTrackingValuesHistoryHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IAsyncEnumerable<UserDailyTrackingValue> Handle(GetCurrentUserDailyTrackingValuesHistory request, CancellationToken cancellationToken)
        {
            var exp = _dbContext.UserDailyTrackingValues
                .AsNoTracking()
                .Where(u => u.UserId.Equals(request.userId))
                .Where(u => u.TrackingValue.UserTrackingId.Equals(request.UserTrackingId))
                .OrderBy(u => u.Day)
                .Include(u => u.TrackingValue)
                .AsSingleQuery()
                .Select(u => new UserDailyTrackingValue
                {
                    UserId = u.UserId,
                    Day = u.Day,
                    UserTrackingValueId = u.UserTrackingValueId,
                    Occurrence = u.Occurrence,
                    Value = u.Value,
                    When = u.When,
                    TrackingValue = u.TrackingValue,
                });

            if (request.EndDate.HasValue)
            {
                exp = exp.Where(userDay => userDay.Day <= request.EndDate);
            }

            if (request.StartDate.HasValue)
            {
                exp = exp.Where(userDay => userDay.Day >= request.StartDate);
            }

            return exp.AsAsyncEnumerable();
        }
    }
}