using System;
using System.Collections.Generic;
using System.Linq;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.UserDailyTrackingValues
{
    public record GetCurrentUserDailyTrackingValuesHistory(string userId, int UserTrackingId, Nullable<DateOnly> StartDate, Nullable<DateOnly> EndDate) :
        IRequest<IAsyncEnumerable<UserDailyTrackingValue>>;
    public class GetCurrentUserDailyTrackingValuesHistoryHandler : RequestHandler<GetCurrentUserDailyTrackingValuesHistory, IAsyncEnumerable<UserDailyTrackingValue>>
    {
        private readonly DietTrackerDbContext _dbContext;

        public GetCurrentUserDailyTrackingValuesHistoryHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override IAsyncEnumerable<UserDailyTrackingValue> Handle(GetCurrentUserDailyTrackingValuesHistory request)
        {
            var exp = _dbContext.UserDailyTrackingValues
                .AsNoTracking()
                .Where(u => u.UserId.Equals(request.userId))
                .Where(u => u.TrackingValue.UserTrackingId.Equals(request.UserTrackingId))
                .OrderBy(u => u.Day)
                .Include(u => u.TrackingValue)
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