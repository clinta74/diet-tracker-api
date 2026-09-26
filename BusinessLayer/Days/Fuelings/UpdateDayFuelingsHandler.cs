using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.Days.Fuelings;

/// <returns>
/// The ids in the request that are not the user's fuelings for that day. When any are returned the
/// request is rejected and nothing is saved.
/// </returns>
public record UpdateDayFuelings(DateTime Day, string UserId, IEnumerable<UserFueling> Fuelings) : IRequest<IReadOnlyList<int>>;
public class UpdateDayFuelingsHandler(DietTrackerDbContext dbContext) : IRequestHandler<UpdateDayFuelings, IReadOnlyList<int>>
{
    private readonly DietTrackerDbContext _dbContext = dbContext;

    public async ValueTask<IReadOnlyList<int>> Handle(UpdateDayFuelings request, CancellationToken cancellationToken)
    {
        var day = request.Day.Date;

        // Only the user's own fuelings for this day can be changed.
        var existing = await _dbContext.UserFuelings
            .Where(fueling => fueling.UserId == request.UserId && fueling.Day == day)
            .ToDictionaryAsync(fueling => fueling.UserFuelingId, cancellationToken);

        var rejectedIds = request.Fuelings
            .Select(fueling => fueling.UserFuelingId)
            .Where(id => id != 0 && !existing.ContainsKey(id))
            .Distinct()
            .ToList();

        if (rejectedIds.Count > 0)
        {
            return rejectedIds;
        }

        foreach (var fueling in request.Fuelings)
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

        return [];
    }
}
