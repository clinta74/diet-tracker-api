using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using MediatR;

namespace diet_tracker_api.CQRS.Plans
{
    public record AddPlan(string Name, int FuelingCount, int MealCount) : IRequest<int>;
    public class AddPlanHandler : IRequestHandler<AddPlan, int>
    {
        private readonly DietTrackerDbContext _dbContext;
        public AddPlanHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> Handle(AddPlan request, CancellationToken cancellationToken)
        {
            var result = _dbContext.Plans
                .Add(new Plan
                {
                    Name = request.Name,
                    MealCount = request.MealCount,
                    FuelingCount = request.FuelingCount,
                });

            await _dbContext.SaveChangesAsync(cancellationToken);

            return result.Entity.PlanId;
        }
    }
}