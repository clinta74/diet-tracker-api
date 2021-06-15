using System.Collections.Generic;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.CQRS.Plans
{
    public record GetPlans() : IRequest<IAsyncEnumerable<Plan>>;
    public class GetPlansHandler : RequestHandler<GetPlans, IAsyncEnumerable<Plan>>
    {
        private readonly DietTrackerDbContext _dbContext;
        public GetPlansHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override IAsyncEnumerable<Plan> Handle(GetPlans request)
        {
            return _dbContext.Plans
                .AsNoTracking()
                .AsAsyncEnumerable();
        }
    }
}