using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.UserTrackings
{
    public record DeleteUserTracking(int UserTrackingId, string UserId) : IRequest<bool>;
    public class DeleteUserTrackingHandler : IRequestHandler<DeleteUserTracking, bool>
    {
        private readonly DietTrackerDbContext _dbContext;

        public DeleteUserTrackingHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<bool> Handle(DeleteUserTracking request, CancellationToken cancellationToken)
        {
            var data = await _dbContext.UserTrackings
                        .AsNoTracking()
                        .Include(u => u.Values)
                        .Where(u => u.UserTrackingId.Equals(request.UserTrackingId))
                        .Where(u => u.UserId.Equals(request.UserId))
                        .SingleOrDefaultAsync(cancellationToken);

            if (data == null)
            {
                throw new ArgumentException($"User Tracking Id ({request.UserTrackingId}) for User Id ({request.UserId}) not found.");
            }

            var values = data.Values.ToList();

            var userValues = await _dbContext.UserDailyTrackingValues
                .AsNoTracking()
                .Where(v => v.TrackingValue.UserTrackingId.Equals(request.UserTrackingId))
                .ToListAsync();

            using var transaction = _dbContext.Database.BeginTransaction();

            _dbContext.UserDailyTrackingValues
                .RemoveRange(userValues);

            _dbContext.UserTrackingValues
                .RemoveRange(values);

            _dbContext.UserTrackings
                .Remove(data);

            var result =  await _dbContext.SaveChangesAsync(cancellationToken) > 0;
            
            await transaction.CommitAsync(cancellationToken);

            return result;
        }
    }
}