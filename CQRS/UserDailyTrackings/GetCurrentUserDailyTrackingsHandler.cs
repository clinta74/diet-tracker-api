using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using diet_tracker_api.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.CQRS.UserDailyTrackings
{
    public record GetCurrentUserDailyTrackings(DateTime day, string userId) : IRequest<IEnumerable<CurrentUserDailyTracking>>;
    public class GetCurrentUserDailyTrackingsHandler : IRequestHandler<GetCurrentUserDailyTrackings, IEnumerable<CurrentUserDailyTracking>>
    {
        private readonly DietTrackerDbContext _dbContext;

        public GetCurrentUserDailyTrackingsHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<CurrentUserDailyTracking>> Handle(GetCurrentUserDailyTrackings request, CancellationToken cancellationToken)
        {
            var data = await _dbContext.UserDailyTrackings
                .AsNoTracking()
                .Where(u => u.Day.Equals(request.day))
                .Where(u => u.UserId.Equals(request.userId))
                .Select(u => new CurrentUserDailyTracking
                {
                    UserId = u.UserId,
                    Day = u.Day,
                    Occurrence = u.Occurrence,
                    UserTrackingId = u.UserTrackingId,
                    Title = u.Tracking.Title,
                    Description = u.Tracking.Description,
                    Values = u.Values
                        .Where(v => !v.TrackingValue.Disabled)
                        .Select(v => new CurrentUserDailyTrackingValue
                        {
                            Name = v.TrackingValue.Name,
                            Order = v.TrackingValue.Order,
                            Value = v.Value,
                            When = v.When,
                        })
                })
                .ToListAsync(cancellationToken);

            return data;
        }
    }
}