using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.CQRS.UserTrackingValues
{
    public record DeleteUserTrackingValue(string UserId, int UserTrackingValueId) : IRequest<bool>;
    public class DeleteUserTrackingValueHandler : IRequestHandler<DeleteUserTrackingValue, bool>
    {
        private readonly DietTrackerDbContext _dbContext;

        public DeleteUserTrackingValueHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> Handle(DeleteUserTrackingValue request, CancellationToken cancellationToken)
        {
            var data = await _dbContext.UserTrackingValues
                .AsNoTracking()
                .Where(p => p.UserTrackingValueId == request.UserTrackingValueId)
                .Where(p => p.Tracking.UserId == request.UserId)
                .FirstOrDefaultAsync(cancellationToken);

            if (data == null)
            {
                throw new ArgumentException($"User Tracking Value Id ({request.UserTrackingValueId}) for User Id ({request.UserId}) not found.");
            }

            _dbContext.Remove(data);

            return await _dbContext.SaveChangesAsync(cancellationToken) == 1;
        }
    }
}