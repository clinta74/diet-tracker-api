using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using LanguageExt.Common;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.Plans
{
    public record UpdatePlan(int PlanId, string Name, int FuelingCount, int MealCount) : IRequest<Result<Plan>>;
    public class UpdatePlanHandler : IRequestHandler<UpdatePlan, Result<Plan>>
    {
        private readonly DietTrackerDbContext _dbContext;
        public UpdatePlanHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<Plan>> Handle(UpdatePlan request, CancellationToken cancellationToken)
        {
            var rowsAffected = await _dbContext.Plans
                .Where(plan => plan.PlanId.Equals(request.PlanId))
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(p => p.Name, request.Name)
                    .SetProperty(p => p.FuelingCount, request.FuelingCount)
                    .SetProperty(p => p.MealCount, request.MealCount),
                    cancellationToken);

            if (rowsAffected == 0)
            {
                var argumentException = new ArgumentException($"Plan Id ({request.PlanId}) not found.");
                return new Result<Plan>(argumentException);
            }

            // Fetch the updated plan to return
            var updatedPlan = await _dbContext.Plans
                .FirstOrDefaultAsync(p => p.PlanId == request.PlanId, cancellationToken);

            return updatedPlan!;
        }
    }
}