using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.CQRS.UserTrackings
{
    public record GetActiveUserTrackings(string UserId) : IRequest<IEnumerable<UserTracking>>;
    public class GetActiveUserTrackingsHandler : IRequestHandler<GetActiveUserTrackings, IEnumerable<UserTracking>>
    {
        private readonly DietTrackerDbContext _dbContext;

        public GetActiveUserTrackingsHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<UserTracking>> Handle(GetActiveUserTrackings request, CancellationToken cancellationToken)
        {
            return await _dbContext.UserTrackings
                .Where(userTracking => userTracking.UserId == request.UserId)
                .Where(userTracking => !userTracking.Removed)
                .Include(userTracking => userTracking.Values.Where(values => !values.Removed))
                .Select(userTracking => new UserTracking
                {
                    UserTrackingId = userTracking.UserTrackingId,
                    UserId = userTracking.UserId,
                    Title = userTracking.Title,
                    Description = userTracking.Description,
                    Occurrences = userTracking.Occurrences,
                    Order = userTracking.Order,
                    Removed = userTracking.Removed,
                    Values = userTracking.Values.Select(v => new UserTrackingValue
                    {
                        UserTrackingValueId = v.UserTrackingId,
                        UserTrackingId = v.UserTrackingId,
                        Name = v.Name,
                        Description = v.Description,
                        Order = v.Order,
                        Removed = v.Removed
                    })
                })
                .ToListAsync();
        }
    }
}