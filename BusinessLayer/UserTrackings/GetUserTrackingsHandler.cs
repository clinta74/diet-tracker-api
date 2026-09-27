using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.UserTrackings;

public record GetUserTrackings(string UserId) : IRequest<IEnumerable<UserTracking>>;
public class GetUserTrackingsHandler(DietTrackerDbContext dbContext) : IRequestHandler<GetUserTrackings, IEnumerable<UserTracking>>
{
    private readonly DietTrackerDbContext _dbContext = dbContext;

    public async ValueTask<IEnumerable<UserTracking>> Handle(GetUserTrackings request, CancellationToken cancellationToken)
    {
        return await _dbContext.UserTrackings
            .Where(userTracking => userTracking.UserId == request.UserId)
            .OrderBy(userTracking => userTracking.Order)
            .ThenBy(userTracking => userTracking.UserTrackingId)
            .Select(userTracking => new UserTracking
            {
                UserTrackingId = userTracking.UserTrackingId,
                UserId = userTracking.UserId,
                Title = userTracking.Title,
                Description = userTracking.Description,
                Occurrences = userTracking.Occurrences,
                Order = userTracking.Order,
                Disabled = userTracking.Disabled,
                UseTime = userTracking.UseTime,
                Values = userTracking.Values!
                    .OrderBy(v => v.Order)
                    .ThenBy(v => v.UserTrackingValueId)
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