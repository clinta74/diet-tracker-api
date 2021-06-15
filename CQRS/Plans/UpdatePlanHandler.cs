using System;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.CQRS.Plans
{
    public record UpdatePlan(int PlanId, string Name, int FuelingCount, int MealCount) : IRequest<Plan>;
    public class UpdatePlanHandler : IRequestHandler<UpdatePlan, Plan>
    {
        private readonly DietTrackerDbContext _dbContext;
        public UpdatePlanHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Plan> Handle(UpdatePlan request, CancellationToken cancellationToken)
        {
            var data = await _dbContext.Plans
                .AsNoTracking()
                .SingleOrDefaultAsync(plan => plan.PlanId == request.PlanId);

            if (data == null)
            {
                throw new ArgumentException($"Plan Id ({request.PlanId}) not found.");
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