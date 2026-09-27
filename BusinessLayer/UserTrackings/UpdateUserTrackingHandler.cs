using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.UserTrackings;

public record UpdateUserTracking(
    string UserId, 
    int UserTrackingId, 
    string Title, 
    string Description, 
    int Occurrences, 
    bool Disabled,
    bool UseTime,
    IEnumerable<UserTrackingValue> Values) : IRequest<UserTracking>;
public class UpdateUserTrackingHandler(DietTrackerDbContext dbContext, ILogger<UpdateUserTrackingHandler> logger) : IRequestHandler<UpdateUserTracking, UserTracking>
{
    private readonly DietTrackerDbContext _dbContext = dbContext;
    private readonly ILogger<UpdateUserTrackingHandler> _logger = logger;

    public async ValueTask<UserTracking> Handle(UpdateUserTracking request, CancellationToken cancellationToken)
    {
        // Verify the tracking exists
        var exists = await _dbContext.UserTrackings
            .Where(u => u.UserTrackingId.Equals(request.UserTrackingId))
            .Where(u => u.UserId == request.UserId)
            .AnyAsync(cancellationToken);

        if (!exists)
        {
            throw new ArgumentException($"UserTrackingId ({request.UserTrackingId}) not found.");
        }

        // Only this tracking's values can change; ids from other trackings (or users) are pruned.
        var ownedValueIds = (await _dbContext.UserTrackingValues
            .Where(value => value.UserTrackingId == request.UserTrackingId)
            .Select(value => value.UserTrackingValueId)
            .ToListAsync(cancellationToken))
            .ToHashSet();

        var ignoredIds = request.Values
            .Select(value => value.UserTrackingValueId)
            .Where(id => id != 0 && !ownedValueIds.Contains(id))
            .Distinct()
            .ToList();

        if (ignoredIds.Count > 0)
        {
            _logger.LogWarning("Ignored tracking value ids not belonging to user {UserId}'s tracking {UserTrackingId}: {UserTrackingValueIds}",
                request.UserId, request.UserTrackingId, ignoredIds);
        }

        var existingValues = request.Values.Where(value => ownedValueIds.Contains(value.UserTrackingValueId)).ToList();

        using var transaction = _dbContext.Database.BeginTransaction();
        
        // Update the main UserTracking
        await _dbContext.UserTrackings
            .Where(u => u.UserTrackingId.Equals(request.UserTrackingId))
            .Where(u => u.UserId == request.UserId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(u => u.Title, request.Title)
                .SetProperty(u => u.Description, request.Description)
                .SetProperty(u => u.Occurrences, request.Occurrences)
                .SetProperty(u => u.Disabled, request.Disabled)
                .SetProperty(u => u.UseTime, request.UseTime),
                cancellationToken);

        // Remove tracking values that are no longer in the request. This runs before new values are
        // added; afterwards their ids would not be in the list and they would be deleted straight away.
        var removeTrackingValueIds = existingValues.Select(value => value.UserTrackingValueId).ToList();
        await _dbContext.UserTrackingValues
            .Where(userTrackingValue => userTrackingValue.Tracking!.UserId == request.UserId)
            .Where(userTrackingValue => userTrackingValue.UserTrackingId == request.UserTrackingId)
            .Where(userTrackingValue => !removeTrackingValueIds.Contains(userTrackingValue.UserTrackingValueId))
            .ExecuteDeleteAsync(cancellationToken);

        // Add new tracking values
        _dbContext.UserTrackingValues
            .AddRange(request.Values
                .Where(userTrackingValue => userTrackingValue.UserTrackingValueId == 0)
                .Select(userTrackingValue => new UserTrackingValue
                {
                    UserTrackingId = request.UserTrackingId,
                    Name = userTrackingValue.Name,
                    Description = userTrackingValue.Description,
                    Disabled = userTrackingValue.Disabled,
                    Order = userTrackingValue.Order,
                    Type = userTrackingValue.Type,
                    Metadata = userTrackingValue.Metadata,
                })
            );

        await _dbContext.SaveChangesAsync(cancellationToken);

        // Update existing tracking values
        foreach (var userTrackingValue in existingValues)
        {
            await _dbContext.UserTrackingValues
                .Where(p => p.UserTrackingValueId == userTrackingValue.UserTrackingValueId)
                .Where(p => p.Tracking!.UserId == request.UserId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(v => v.Name, userTrackingValue.Name)
                    .SetProperty(v => v.Description, userTrackingValue.Description)
                    .SetProperty(v => v.Order, userTrackingValue.Order)
                    .SetProperty(v => v.Type, userTrackingValue.Type)
                    .SetProperty(v => v.Disabled, userTrackingValue.Disabled),
                    cancellationToken);

            // Delete and recreate metadata
            await _dbContext.UserTrackingValueMetadata
                .Where(p => p.UserTrackingValueId == userTrackingValue.UserTrackingValueId)
                .ExecuteDeleteAsync(cancellationToken);
            
            _dbContext.UserTrackingValueMetadata
                .AddRange((userTrackingValue.Metadata ?? [])
                    .Select(m => new UserTrackingValueMetadata
                    {
                        Key = m.Key,
                        Value = m.Value,
                        UserTrackingValueId = userTrackingValue.UserTrackingValueId
                    }));
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        // Fetch and return the updated tracking
        var updatedTracking = await _dbContext.UserTrackings
            .AsNoTracking()
            .FirstAsync(u => u.UserTrackingId == request.UserTrackingId, cancellationToken);

        return updatedTracking;
    }
}