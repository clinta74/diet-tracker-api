using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.Plans;

public record GetPlans() : IStreamRequest<Plan>;
public class GetPlansHandler(DietTrackerDbContext dbContext) : IStreamRequestHandler<GetPlans, Plan>
{
    private readonly DietTrackerDbContext _dbContext = dbContext;

    public IAsyncEnumerable<Plan> Handle(GetPlans request, CancellationToken cancellationToken)
    {
        return _dbContext.Plans
            .AsNoTracking()
            .AsAsyncEnumerable();
    }
}