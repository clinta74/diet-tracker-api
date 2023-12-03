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
            var data = await _dbContext.Plans
                .AsNoTracking()
                .SingleOrDefaultAsync(plan => plan.PlanId.Equals(request.PlanId));

            if (data == null)
            {
                var argumentException = new ArgumentException($"Plan Id ({request.PlanId}) not found.");
                return new Result<Plan>(argumentException);
            }

            _dbContext.Plans.Update(data with
            {
                Name = request.Name,
                FuelingCount = request.FuelingCount,
                MealCount = request.MealCount,
            });

            await _dbContext.SaveChangesAsync(cancellationToken);

            return data;
        }
    }
}