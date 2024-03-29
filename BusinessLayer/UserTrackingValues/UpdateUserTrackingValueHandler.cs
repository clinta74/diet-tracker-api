using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.UserTrackingValues
{
    public record UpdateUserTrackingValue(int UserTrackingValueId, string UserId, string Name, string Description, int Order, UserTrackingType Type, bool Disabled, IEnumerable<UserTrackingValueMetadata> Metadata) : IRequest<bool>;
    public class UpdateUserTrackingValueHandler : IRequestHandler<UpdateUserTrackingValue, bool>
    {
        private readonly DietTrackerDbContext _dbContext;

        public UpdateUserTrackingValueHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> Handle(UpdateUserTrackingValue request, CancellationToken cancellationToken)
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

            _dbContext.Update(data with {
                Name = request.Name,
                Description = request.Description,
                Order = request.Order,
                Type = request.Type,
                Disabled = request.Disabled,
                Metadata = request.Metadata.ToArray(),
            });

            return await _dbContext.SaveChangesAsync(cancellationToken) == 1;
        }
    }
}