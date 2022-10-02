using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using LanguageExt.Common;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.Plans
{
    public record GetPlanById(int PlanId) : IRequest<Result<Plan>>;
    public class GetByIdHandler : IRequestHandler<Plans.GetPlanById, Result<Plan>>
    {
        private readonly DietTrackerDbContext _dbContext;
        public GetByIdHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Result<Plan>> Handle(Plans.GetPlanById request, CancellationToken cancellationToken)
        {
            var data = await _dbContext.Plans
                .AsNoTracking()
                .SingleOrDefaultAsync(plan => plan.PlanId.Equals(request.PlanId), cancellationToken);

            if (data == null)
            {
                var argumentException = new ArgumentException($"Plan Id ({request.PlanId}) not found.");
                return new Result<Plan>(argumentException);
            }
            return data;
        }
    }
}