using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.CQRS.UserDailyTrackings
{
    public record UpdateUserDailyTracking(DateTime Day, string UserId, int UserTrackingId, int Occurance, int Value, DateTime When) : IRequest<UserDailyTracking>;
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

            var result = _dbContext.UserDailyTrackings
                .Update(data with
                {
                    Value = request.Value,
                    When = request.When
                });

            await _dbContext.SaveChangesAsync(cancellationToken);

            return result.Entity;
        }
    }
}