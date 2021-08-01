using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.UserTrackings
{
    public record GetUserTracking(string UserId, int UserTrackingId) : IRequest<UserTracking>;
    public class GetUserTrackingHandler : IRequestHandler<GetUserTracking, UserTracking>
    {
        private readonly DietTrackerDbContext _dbContext;

        public GetUserTrackingHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<UserTracking> Handle(GetUserTracking request, CancellationToken cancellationToken)
        {
            return await _dbContext.UserTrackings
                .Where(userTracking => userTracking.UserId == request.UserId)
                .Where(userTracking => userTracking.UserTrackingId == request.UserTrackingId)
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
                            Type = v.Type,
                            Order = v.Order,
                            Disabled = v.Disabled,
                            Metadata = v.Metadata,
                        })
                })
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}