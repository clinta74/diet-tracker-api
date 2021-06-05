using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.CQRS.UserTrackings
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
                        .Where(u => u.UserTrackingId.Equals(request.UserTrackingId))
                        .Where(u => u.UserId.Equals(request.UserId))
                        .SingleOrDefaultAsync(cancellationToken);

            if (data == null)
            {
                throw new ArgumentException($"User Tracking Id ({request.UserTrackingId}) for User Id ({request.UserId}) not found.");
            }

            var values = data.Values.ToList();

            using var transaction = _dbContext.Database.BeginTransaction();

            _dbContext.UserTrackingValues
                .RemoveRange(values);

            _dbContext.UserTrackings
                .Remove(data);

            var result =  await _dbContext.SaveChangesAsync(cancellationToken) == 1;
            
            await transaction.CommitAsync();

            return result;
        }
    }
}