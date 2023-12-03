using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using LanguageExt.Common;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.Plans
{
    public record DeletePlan(int PlanId) : IRequest<Result<bool>>;
    public class DeletePlanHandler : IRequestHandler<DeletePlan, Result<bool>>
    {
        private readonly DietTrackerDbContext _dbContext;
        public DeletePlanHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<bool>> Handle(DeletePlan request, CancellationToken cancellationToken)
        {
            var data = await _dbContext.Plans
                .AsNoTracking()
                .SingleOrDefaultAsync(fueling => fueling.PlanId == request.PlanId);

            if (data == null)
            {
                var argumentException = new ArgumentException($"Plan Id ({request.PlanId}) not found.");
                return new Result<bool>(argumentException);
            }
            
            _dbContext.Plans.Remove(data);

            return await _dbContext.SaveChangesAsync(cancellationToken) == 1;
        }
    }
}