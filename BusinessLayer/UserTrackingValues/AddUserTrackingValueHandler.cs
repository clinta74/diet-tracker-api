using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.UserTrackingValues;

/// <returns>The new value's id, or null when the tracking is not one of the user's.</returns>
public record AddUserTrackingValue(string UserId, int UserTrackingId, string? Name, string? Description, int Order, UserTrackingType Type, bool Disabled, IEnumerable<UserTrackingValueMetadata> Metadata) : IRequest<int?>;
public class AddUserTrackingValueHandler(DietTrackerDbContext dbContext) : IRequestHandler<AddUserTrackingValue, int?>
{
    private readonly DietTrackerDbContext _dbContext = dbContext;

    public async ValueTask<int?> Handle(AddUserTrackingValue request, CancellationToken cancellationToken)
    {
        var ownsTracking = await _dbContext.UserTrackings
            .AnyAsync(tracking => tracking.UserTrackingId == request.UserTrackingId && tracking.UserId == request.UserId, cancellationToken);

        if (!ownsTracking)
        {
            return null;
        }

        var data = _dbContext.UserTrackingValues
            .Add(new UserTrackingValue
            {
                UserTrackingId = request.UserTrackingId,
                Name = request.Name,
                Description = request.Description,
                Order = request.Order,
                Type = request.Type,
                Disabled = request.Disabled,
                Metadata = request.Metadata.ToArray(),
            });

        await _dbContext.SaveChangesAsync(cancellationToken);

        return data.Entity.UserTrackingValueId;
    }
}