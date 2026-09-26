using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.Days;

public record GetCurrentUserPlan(string UserId) : IRequest<Plan>;
public class GetCurrentUserPlanHandler(DietTrackerDbContext dbContext, IMediator mediator) : IRequestHandler<GetCurrentUserPlan, Plan>
{
    private readonly DietTrackerDbContext _dbContext = dbContext;
    private readonly IMediator _mediator = mediator;

    public async ValueTask<Plan> Handle(GetCurrentUserPlan request, CancellationToken cancellationToken)
    {
        var plan = await _dbContext.UserPlans
                .OrderByDescending(up => up.Start)
                .Where(up => up.UserId == request.UserId)
                .Select(up => up.Plan!)
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

        if (plan == null)
        {
            throw new ArgumentException($"User ID ({request.UserId}) has no selected plan.");
        }

        return plan;
    }
}