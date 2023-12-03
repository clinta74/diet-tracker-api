using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.UserTrackings
{
    public record GetUserTrackings(string UserId) : IRequest<IEnumerable<UserTracking>>;
    public class GetUserTrackingsHandler : IRequestHandler<GetUserTrackings, IEnumerable<UserTracking>>
    {
        private readonly DietTrackerDbContext _dbContext;

        public GetUserTrackingsHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<UserTracking>> Handle(GetUserTrackings request, CancellationToken cancellationToken)
        {
            return await _dbContext.UserTrackings
                .Where(userTracking => userTracking.UserId == request.UserId)
                .Select(userTracking => new UserTracking
                {
                    UserTrackingId = userTracking.UserTrackingId,
                    UserId = userTracking.UserId,
                    Title = userTracking.Title,
                    Description = userTracking.Description,
                    Occurrences = userTracking.Occurrences,
                    Order = userTracking.Order,
                    Disabled = userTracking.Disabled,
                    Values = userTracking.Values
                        .Select(v => new UserTrackingValue
                        {
                            UserTrackingValueId = v.UserTrackingValueId,
                            UserTrackingId = v.UserTrackingId,
                            Name = v.Name,
                            Description = v.Description,
                            Order = v.Order,
                            Disabled = v.Disabled,
                            Type = v.Type,
                            Metadata = v.Metadata,
                        })
                })
                .ToListAsync(cancellationToken);
        }
    }
}