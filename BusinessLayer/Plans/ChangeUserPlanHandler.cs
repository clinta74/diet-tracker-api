using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;

namespace diet_tracker_api.BusinessLayer.Plans
{
    public record ChangeUserPlan(string UserId, int PlanId) : IRequest<int>;

    public class ChangeUserPlanHandler : IRequestHandler<ChangeUserPlan, int>
    {
        private readonly DietTrackerDbContext ctx;
        public ChangeUserPlanHandler(DietTrackerDbContext context)
        {
            ctx = context;
        }

        public async Task<int> Handle(ChangeUserPlan request, CancellationToken cancellationToken)
        {
            ctx.UserPlans.Add(new UserPlan
            {
                UserId = request.UserId,
                PlanId = request.PlanId,
                Start = DateTime.Now,
            });

            await ctx.SaveChangesAsync(cancellationToken);

            return request.PlanId;
        }
    }
}