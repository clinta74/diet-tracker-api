using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.CQRS.UserDailyTrackings
{
    public record GetUserDailyTrackings(DateTime day, string userId) : IRequest<IEnumerable<UserDailyTracking>>;
    public class GetUserDailyTrackingsHandler : IRequestHandler<GetUserDailyTrackings, IEnumerable<UserDailyTracking>>
    {
        private readonly DietTrackerDbContext _dbContext;

        public GetUserDailyTrackingsHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<UserDailyTracking>> Handle(GetUserDailyTrackings request, CancellationToken cancellationToken)
        {
            var data = await _dbContext.UserDailyTrackings
                .AsNoTracking()
                .Where(u => u.Day.Equals(request.day))
                .Where(u => u.UserId.Equals(request.userId))
                .ToListAsync(cancellationToken);

            return data;
        }
    }
}