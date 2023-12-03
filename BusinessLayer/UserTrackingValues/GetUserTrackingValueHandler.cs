using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.UserTrackingValues
{
    public record GetUserTrackingValue(int UserTrackingValueId, string UserId) : IRequest<UserTrackingValue>;
    public class GetUserTrackingValueHandler : IRequestHandler<GetUserTrackingValue, UserTrackingValue>
    {
        private readonly DietTrackerDbContext _dbContext;

        public GetUserTrackingValueHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<UserTrackingValue> Handle(GetUserTrackingValue request, CancellationToken cancellationToken)
        {
            var data = await _dbContext.UserTrackingValues
                .AsNoTracking()
                .Where(p => p.UserTrackingValueId == request.UserTrackingValueId)
                .Where(p => p.Tracking.UserId == request.UserId)
                .Select(p => new UserTrackingValue
                {
                    UserTrackingValueId = p.UserTrackingValueId,
                    UserTrackingId = p.UserTrackingId,
                    Name = p.Name,
                    Description = p.Description,
                    Order = p.Order,
                    Type = p.Type,
                    Disabled = p.Disabled,
                    Metadata = p.Metadata,                 
                })
                .FirstOrDefaultAsync(cancellationToken);

            return data;
        }
    }
}