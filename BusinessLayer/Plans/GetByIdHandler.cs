#nullable enable
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.Plans
{
    public record GetPlanById(int PlanId) : IRequest<Plan?>;
    public class GetByIdHandler : IRequestHandler<GetPlanById, Plan?>
    {
        private readonly DietTrackerDbContext _dbContext;
        public GetByIdHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async ValueTask<Plan?> Handle(GetPlanById request, CancellationToken cancellationToken)
        {
            return await _dbContext.Plans
                .AsNoTracking()
                .SingleOrDefaultAsync(plan => plan.PlanId.Equals(request.PlanId), cancellationToken);
        }
    }
}