using System;
using System.Collections;
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
    public record UpdateUserDailyTracking(DateTime Day, string UserId, int UserTrackingId, int Occurance, IEnumerable<CurrentUserDailyTrackingValueRequest> values) : IRequest<UserDailyTracking>;
    public class UpdateUserDailyTrackingHandler : IRequestHandler<UpdateUserDailyTracking, UserDailyTracking>
    {
        private readonly DietTrackerDbContext _dbContext;

        public UpdateUserDailyTrackingHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<UserDailyTracking> Handle(UpdateUserDailyTracking request, CancellationToken cancellationToken)
        {
            var data = await _dbContext.UserDailyTrackings
                        .AsNoTracking()
                        .Where(u => u.Day.Equals(request.Day))
                        .Where(u => u.UserId.Equals(request.UserId))
                        .Where(u => u.UserTrackingId.Equals(request.UserTrackingId))
                        .Where(u => u.Occurrence.Equals(request.Occurance))
                        .SingleOrDefaultAsync(cancellationToken);

            if (data == null) return null;

            var values = request.values.Select(v => new UserDailyTrackingValue
            {
                Day = request.Day,
                UserId = request.UserId,
                UserTrackingId = request.UserTrackingId,
                Occurrence = request.UserTrackingId,
                Value = v.Value,
                When = v.When,
            }).ToList();

            var result = _dbContext.UserDailyTrackings
                .Update(data with
                {
                    Values = values
                });

            await _dbContext.SaveChangesAsync(cancellationToken);

            return result.Entity;
        }
    }
}