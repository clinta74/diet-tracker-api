using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.UserTrackingValues;

public record UpdateUserTrackingValue(int UserTrackingValueId, string UserId, string? Name, string? Description, int Order, UserTrackingType Type, bool Disabled, IEnumerable<UserTrackingValueMetadata> Metadata) : IRequest<bool>;
public class UpdateUserTrackingValueHandler(DietTrackerDbContext dbContext) : IRequestHandler<UpdateUserTrackingValue, bool>
{
    private readonly DietTrackerDbContext _dbContext = dbContext;

    public async ValueTask<bool> Handle(UpdateUserTrackingValue request, CancellationToken cancellationToken)
    {
        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        var rowsAffected = await _dbContext.UserTrackingValues
            .Where(p => p.UserTrackingValueId == request.UserTrackingValueId)
            .Where(p => p.Tracking!.UserId == request.UserId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(v => v.Name, request.Name)
                .SetProperty(v => v.Description, request.Description)
                .SetProperty(v => v.Order, request.Order)
                .SetProperty(v => v.Type, request.Type)
                .SetProperty(v => v.Disabled, request.Disabled),
                cancellationToken);

        // Not one of the user's values; the controller returns 404.
        if (rowsAffected == 0)
        {
            return false;
        }

        await _dbContext.UserTrackingValueMetadata
            .Where(metadata => metadata.UserTrackingValueId == request.UserTrackingValueId)
            .ExecuteDeleteAsync(cancellationToken);

        _dbContext.UserTrackingValueMetadata.AddRange(request.Metadata.Select(metadata => new UserTrackingValueMetadata
        {
            UserTrackingValueId = request.UserTrackingValueId,
            Key = metadata.Key,
            Value = metadata.Value,
        }));

        await _dbContext.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);

        return true;
    }
}