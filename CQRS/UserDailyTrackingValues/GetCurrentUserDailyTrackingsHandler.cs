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

namespace diet_tracker_api.CQRS.UserDailyTrackingValues
{
    public record GetCurrentUserDailyTrackings(DateTime day, string userId) : IRequest<IEnumerable<CurrentUserDailyTrackingValue>>;
    public class GetCurrentUserDailyTrackingsHandler : IRequestHandler<GetCurrentUserDailyTrackings, IEnumerable<CurrentUserDailyTrackingValue>>
    {
        private readonly DietTrackerDbContext _dbContext;

        public GetCurrentUserDailyTrackingsHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<CurrentUserDailyTrackingValue>> Handle(GetCurrentUserDailyTrackings request, CancellationToken cancellationToken)
        {
            var data = await _dbContext.UserDailyTrackingValues
                .AsNoTracking()
                .Where(u => u.Day.Equals(request.day))
                .Where(u => u.UserId.Equals(request.userId))
                .Select(u => new CurrentUserDailyTrackingValue
                {
                    UserId = u.UserId,
                    Day = u.Day,
                    Occurrence = u.Occurrence,
                    UserTrackingValueId = u.UserTrackingValueId,
                    Name = u.TrackingValue.Name,
                    Description = u.TrackingValue.Description,
                    Value = u.Value,
                    When = u.When
                })
                .ToListAsync(cancellationToken);

            return data;
        }
    }
}