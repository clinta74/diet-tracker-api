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
                    Value = u.Value,
                    When = u.When,
                    Occurance = u.Occurrence,
                    UserTrackingId = u.UserTrackingId,
                    Name = u.Tracking.Name,
                    Description = u.Tracking.Description,
                })
                .ToListAsync(cancellationToken);

            return data;
        }
    }
}