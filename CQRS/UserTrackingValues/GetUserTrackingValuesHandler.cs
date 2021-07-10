using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.CQRS.UserTrackingValues
{
    public record GetUserTrackingValues(string UserId, int UserTrackingId) : IRequest<IEnumerable<UserTrackingValue>>;
    public class GetUserTrackingValuesHandler : IRequestHandler<GetUserTrackingValues, IEnumerable<UserTrackingValue>>
    {
        private readonly DietTrackerDbContext _dbContext;

        public GetUserTrackingValuesHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<UserTrackingValue>> Handle(GetUserTrackingValues request, CancellationToken cancellationToken)
        {
            var data = await _dbContext.UserTrackingValues
                .AsNoTracking()
                .Where(p => p.UserTrackingId == request.UserTrackingId)
                .Where(p => p.Tracking.UserId == request.UserId)
                .Select(p => new UserTrackingValue
                {
                    UserTrackingValueId = p.UserTrackingValueId,
                    UserTrackingId = p.UserTrackingId,
                    Name = p.Name,
                    Description = p.Description,
                    Order = p.Order,
                    Type = p.Type,
                    Min = p.Min,
                    Max = p.Max,
                    Disabled = p.Disabled           
                })
                .ToListAsync();

            return data;
        }
    }
}