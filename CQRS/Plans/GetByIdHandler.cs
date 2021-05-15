using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.CQRS.Plans
{
    public record GetPlanById(int planId) : IRequest<Plan>;
    public class GetByIdHandler : IRequestHandler<Plans.GetPlanById, Plan>
    {
        private readonly DietTrackerDbContext ctx;
        public GetByIdHandler(DietTrackerDbContext context)
        {
            ctx = context;
        }

        public Task<Plan> Handle(Plans.GetPlanById request, CancellationToken cancellationToken)
        {
            return ctx.Plans
                .Where(plan => plan.PlanId == request.planId)
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}