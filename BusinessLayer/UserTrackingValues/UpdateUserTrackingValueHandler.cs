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
            var rowsAffected = await _dbContext.UserTrackingValues
                .Where(p => p.UserTrackingValueId == request.UserTrackingValueId)
                .Where(p => p.Tracking.UserId == request.UserId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(v => v.Name, request.Name)
                    .SetProperty(v => v.Description, request.Description)
                    .SetProperty(v => v.Order, request.Order)
                    .SetProperty(v => v.Type, request.Type)
                    .SetProperty(v => v.Disabled, request.Disabled)
                    .SetProperty(v => v.Metadata, request.Metadata.ToArray()),
                    cancellationToken);

            if (rowsAffected == 0)
            {
                throw new ArgumentException($"User Tracking Value Id ({request.UserTrackingValueId}) for User Id ({request.UserId}) not found.");
            }

            return rowsAffected == 1;
        }
    }
}