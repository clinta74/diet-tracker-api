using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.CQRS
{
    public class Plans
    {
        public record GetPlanById(int planId) : IRequest<Plan>;

        public class GetByIdHandler : IRequestHandler<GetPlanById, Plan>
        {
            private readonly DietTrackerDbContext ctx;
            public GetByIdHandler(DietTrackerDbContext context)
            {
                ctx = context;
            }

            public Task<Plan> Handle(GetPlanById request, CancellationToken cancellationToken)
            {
                return ctx.Plans
                    .Where(plan => plan.PlanId == request.planId)
                    .AsNoTracking()
                    .FirstOrDefaultAsync();
            }
        }

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

                await ctx.SaveChangesAsync();

                return request.PlanId;
            }
        }
    }
}