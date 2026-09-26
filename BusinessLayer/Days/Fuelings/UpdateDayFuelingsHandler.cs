using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.Days.Fuelings;

public record UpdateDayFuelings(DateTime Day, string UserId, IEnumerable<UserFueling> Fuelings) : IRequest<Unit>;
public class UpdateDayFuelingsHandler(DietTrackerDbContext dbContext, ILogger<UpdateDayFuelingsHandler> logger) : IRequestHandler<UpdateDayFuelings, Unit>
{
    private readonly DietTrackerDbContext _dbContext = dbContext;
    private readonly ILogger<UpdateDayFuelingsHandler> _logger = logger;

    public async ValueTask<Unit> Handle(UpdateDayFuelings request, CancellationToken cancellationToken)
    {
        var day = request.Day.Date;

        // Only the user's own fuelings for this day can be changed; any other ids are pruned.
        var existing = await _dbContext.UserFuelings
            .Where(fueling => fueling.UserId == request.UserId && fueling.Day == day)
            .ToDictionaryAsync(fueling => fueling.UserFuelingId, cancellationToken);

        var ignoredIds = request.Fuelings
            .Select(fueling => fueling.UserFuelingId)
            .Where(id => id != 0 && !existing.ContainsKey(id))
            .Distinct()
            .ToList();

        if (ignoredIds.Count > 0)
        {
            _logger.LogWarning("Ignored fueling ids not belonging to user {UserId} on {Day:yyyy-MM-dd}: {FuelingIds}",
                request.UserId, day, ignoredIds);
        }

        foreach (var fueling in request.Fuelings.Where(fueling => fueling.UserFuelingId == 0 || existing.ContainsKey(fueling.UserFuelingId)))
        {
            // A fueling is only kept when it has a name; clearing the name removes it.
            var name = fueling.Name?.Trim();
            var hasName = !string.IsNullOrEmpty(name);

            if (fueling.UserFuelingId == 0)
            {
                if (hasName)
                {
                    _dbContext.UserFuelings.Add(new UserFueling
                    {
                        UserId = request.UserId,
                        Day = day,
                        Name = name,
                        When = fueling.When,
                    });
                }
            }
            else
            {
                var current = existing[fueling.UserFuelingId];

                if (hasName)
                {
                    _dbContext.Entry(current).CurrentValues.SetValues(current with { Name = name, When = fueling.When });
                }
                else
                {
                    _dbContext.UserFuelings.Remove(current);
                }
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
