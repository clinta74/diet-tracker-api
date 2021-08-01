using System;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.Plans
{
    public record DeletePlan(int PlanId) : IRequest<bool>;
    public class DeletePlanHandler : IRequestHandler<DeletePlan, bool>
    {
        private readonly DietTrackerDbContext _dbContext;
        public DeletePlanHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> Handle(DeletePlan request, CancellationToken cancellationToken)
        {
            var data = await _dbContext.Plans
                .AsNoTracking()
                .SingleOrDefaultAsync(fueling => fueling.PlanId == request.PlanId);

            if (data == null)
            {
                throw new ArgumentException($"Fueling Id ({request.PlanId}) not found.");
            }
            
            _dbContext.Plans.Remove(data);

            return await _dbContext.SaveChangesAsync(cancellationToken) == 1;
        }
    }
}