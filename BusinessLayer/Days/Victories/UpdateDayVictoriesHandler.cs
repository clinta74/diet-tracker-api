using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.Days.Victories;

public record UpdateDayVictories(DateTime Day, string UserId, IEnumerable<Victory> Victories) : IRequest<Unit>;
public class UpdateDayVictoriesHandler(DietTrackerDbContext dbContext, ILogger<UpdateDayVictoriesHandler> logger) : IRequestHandler<UpdateDayVictories, Unit>
{
    private readonly DietTrackerDbContext _dbContext = dbContext;
    private readonly ILogger<UpdateDayVictoriesHandler> _logger = logger;

    public async ValueTask<Unit> Handle(UpdateDayVictories request, CancellationToken cancellationToken)
    {
        var day = request.Day.Date;

        // A day's victories are the user's non-scale victories dated that day (what GetDayVictories returns).
        // Only those can be changed; any other ids are pruned.
        var existing = await _dbContext.Victories
            .Where(victory => victory.UserId == request.UserId && victory.Type == VictoryType.NonScale && victory.When == day)
            .ToDictionaryAsync(victory => victory.VictoryId, cancellationToken);

        var ignoredIds = request.Victories
            .Select(victory => victory.VictoryId)
            .Where(id => id != 0 && !existing.ContainsKey(id))
            .Distinct()
            .ToList();

        if (ignoredIds.Count > 0)
        {
            _logger.LogWarning("Ignored victory ids not belonging to user {UserId} on {Day:yyyy-MM-dd}: {VictoryIds}",
                request.UserId, day, ignoredIds);
        }

        foreach (var victory in request.Victories.Where(victory => victory.VictoryId == 0 || existing.ContainsKey(victory.VictoryId)))
        {
            // A victory is only kept when it has a name; clearing the name removes it.
            var name = victory.Name?.Trim();
            var hasName = !string.IsNullOrEmpty(name);

            if (victory.VictoryId == 0)
            {
                if (hasName)
                {
                    _dbContext.Victories.Add(new Victory
                    {
                        UserId = request.UserId,
                        Name = name,
                        When = day,
                        Type = VictoryType.NonScale,
                    });
                }
            }
            else
            {
                var current = existing[victory.VictoryId];

                if (hasName)
                {
                    _dbContext.Entry(current).CurrentValues.SetValues(current with { Name = name });
                }
                else
                {
                    _dbContext.Victories.Remove(current);
                }
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
