using diet_tracker_api.DataLayer;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.Plans;

/// <returns>False when the plan does not exist.</returns>
public record DeletePlan(int PlanId) : IRequest<bool>;
public class DeletePlanHandler(DietTrackerDbContext dbContext) : IRequestHandler<DeletePlan, bool>
{
    private readonly DietTrackerDbContext _dbContext = dbContext;

    public async ValueTask<bool> Handle(DeletePlan request, CancellationToken cancellationToken)
    {
        return await _dbContext.Plans
            .Where(plan => plan.PlanId == request.PlanId)
            .ExecuteDeleteAsync(cancellationToken) == 1;
    }
}