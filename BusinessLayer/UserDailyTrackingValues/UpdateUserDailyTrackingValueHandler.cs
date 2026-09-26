using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.UserDailyTrackingValues;

public record UpdateUserDailyTrackingValue(int UserTrackingValueId, int Occurance, decimal Value, DateTime? When);
public record UpdateUserDailyTrackingValues(DateTime Day, string UserId, UpdateUserDailyTrackingValue[] Values) : IRequest<IEnumerable<UserDailyTrackingValue>>;
public class UpdateUserDailyTrackingValueHandler(DietTrackerDbContext dbContext, ILogger<UpdateUserDailyTrackingValueHandler> logger) : IRequestHandler<UpdateUserDailyTrackingValues, IEnumerable<UserDailyTrackingValue>>
{
    private readonly DietTrackerDbContext _dbContext = dbContext;
    private readonly ILogger<UpdateUserDailyTrackingValueHandler> _logger = logger;

    public async ValueTask<IEnumerable<UserDailyTrackingValue>> Handle(UpdateUserDailyTrackingValues request, CancellationToken cancellationToken)
    {

        // Values can only be recorded against the user's own tracking values; other ids are pruned.
        var requestedIds = request.Values.Select(value => value.UserTrackingValueId).Distinct().ToList();
        var ownedIds = (await _dbContext.UserTrackingValues
            .Where(value => requestedIds.Contains(value.UserTrackingValueId) && value.Tracking!.UserId == request.UserId)
            .Select(value => value.UserTrackingValueId)
            .ToListAsync(cancellationToken))
            .ToHashSet();

        var ignoredIds = requestedIds.Where(id => !ownedIds.Contains(id)).ToList();
        if (ignoredIds.Count > 0)
        {
            _logger.LogWarning("Ignored tracking value ids not belonging to user {UserId} on {Day:yyyy-MM-dd}: {UserTrackingValueIds}",
                request.UserId, request.Day, ignoredIds);
        }

        var results = new List<UserDailyTrackingValue>();
        using var transaction = _dbContext.Database.BeginTransaction();

        foreach (var value in request.Values.Where(value => ownedIds.Contains(value.UserTrackingValueId)))
        {
            var rowsAffected = await _dbContext.UserDailyTrackingValues
                .Where(u => u.Day.Equals(request.Day))
                .Where(u => u.UserId.Equals(request.UserId))
                .Where(u => u.UserTrackingValueId.Equals(value.UserTrackingValueId))
                .Where(u => u.Occurrence.Equals(value.Occurance))
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(u => u.Value, value.Value)
                    .SetProperty(u => u.When, value.When),
                    cancellationToken);

            if (rowsAffected == 0)
            {
                var result = _dbContext.UserDailyTrackingValues
                    .Add(new UserDailyTrackingValue
                    {
                        Day = request.Day,
                        UserId = request.UserId,
                        UserTrackingValueId = value.UserTrackingValueId,
                        Occurrence = value.Occurance,
                        Value = value.Value,
                        When = value.When,
                    });

                await _dbContext.SaveChangesAsync(cancellationToken);

                results.Add(result.Entity);
            }
            else
            {
                // Fetch the updated entity to add to results
                var updated = await _dbContext.UserDailyTrackingValues
                    .AsNoTracking()
                    .Where(u => u.Day.Equals(request.Day))
                    .Where(u => u.UserId.Equals(request.UserId))
                    .Where(u => u.UserTrackingValueId.Equals(value.UserTrackingValueId))
                    .Where(u => u.Occurrence.Equals(value.Occurance))
                    .FirstAsync(cancellationToken);

                results.Add(updated);
            }
        }
        await transaction.CommitAsync(cancellationToken);

        return results;
    }
}