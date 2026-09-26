using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.Days.Fuelings;

public record UpdateDayFuelings(DateTime Day, string UserId, IEnumerable<UserFueling> Fuelings) : IRequest<Unit>;
public class UpdateDayFuelingsHandler(DietTrackerDbContext dbContext) : IRequestHandler<UpdateDayFuelings, Unit>
{
    private readonly DietTrackerDbContext _dbContext = dbContext;

    public async ValueTask<Unit> Handle(UpdateDayFuelings request, CancellationToken cancellationToken)
    {
        var day = request.Day.Date;

        // Only the user's own fuelings for this day can be changed; other ids in the request are ignored.
        var existing = await _dbContext.UserFuelings
            .Where(fueling => fueling.UserId == request.UserId && fueling.Day == day)
            .ToDictionaryAsync(fueling => fueling.UserFuelingId, cancellationToken);

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
            else if (existing.TryGetValue(fueling.UserFuelingId, out var current))
            {
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
