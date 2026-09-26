using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.Days;

/// <summary>
/// The plan in effect on a day: the latest plan started on or before it, or the user's first plan
/// when the day predates all of them.
/// </summary>
public record GetUserPlanForDay(string UserId, DateTime Day) : IRequest<Plan>;
public class GetUserPlanForDayHandler(DietTrackerDbContext dbContext) : IRequestHandler<GetUserPlanForDay, Plan>
{
    private readonly DietTrackerDbContext _dbContext = dbContext;

    public async ValueTask<Plan> Handle(GetUserPlanForDay request, CancellationToken cancellationToken)
    {
        // UserPlan.Start is stored in UTC; any plan started before the end of the day applies to it.
        var endOfDay = DateTime.SpecifyKind(request.Day.Date.AddDays(1), DateTimeKind.Utc);

        var userPlans = _dbContext.UserPlans
            .AsNoTracking()
            .Where(userPlan => userPlan.UserId == request.UserId);

        var plan = await userPlans
                .Where(userPlan => userPlan.Start < endOfDay)
                .OrderByDescending(userPlan => userPlan.Start)
                .Select(userPlan => userPlan.Plan!)
                .FirstOrDefaultAsync(cancellationToken)
            ?? await userPlans
                .OrderBy(userPlan => userPlan.Start)
                .Select(userPlan => userPlan.Plan!)
                .FirstOrDefaultAsync(cancellationToken);

        return plan ?? throw new ArgumentException($"User ID ({request.UserId}) has no selected plan.");
    }
}
