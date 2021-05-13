using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.CQRS.UserDailyTrackings
{
    public record UpdateUserDailyTracking(DateTime Day, string UserId, int UserTrackingId, int Occurance, int Value, DateTime When) : IRequest<bool>;
    public class UpdateUserDailyTrackingHandler : IRequestHandler<UpdateUserDailyTracking, bool>
    {
        private readonly DietTrackerDbContext _dbContext;

        public UpdateUserDailyTrackingHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> Handle(UpdateUserDailyTracking request, CancellationToken cancellationToken)
        {
            var data = await _dbContext.UserDailyTrackings
                        .AsNoTracking()
                        .Where(u => u.Day.Equals(request.Day))
                        .Where(u => u.UserTrackingId.Equals(request.UserTrackingId))
                        .Where(u => u.UserId.Equals(request.UserId))
                        .Where(u => u.Occurance.Equals(request.Occurance))
                        .SingleOrDefaultAsync(cancellationToken);

            if (data == null) return false;

            _dbContext.UserDailyTrackings
                .Update(data with
                {
                    Value = request.Value,
                    When = request.When
                });

            return await _dbContext.SaveChangesAsync(cancellationToken) == 1;
        }
    }
}