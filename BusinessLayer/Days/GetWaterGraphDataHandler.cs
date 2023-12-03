using System.Collections.Generic;
using System.Linq;
using System.Threading;
using diet_tracker_api.DataLayer;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.Days
{
    public record GetWaterGraphData(string UserId, DateTime StartDate, DateTime? EndDate) : 
    GetGraphData(UserId, StartDate, EndDate), IStreamRequest<GraphValue>;
    public class GetWaterGraphDataHandler : IStreamRequestHandler<GetWaterGraphData, GraphValue>
    {
        private readonly DietTrackerDbContext _dbContext;

        public GetWaterGraphDataHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IAsyncEnumerable<GraphValue> Handle(GetWaterGraphData request, CancellationToken cancellationToken)
        {
            var exp = _dbContext.UserDays
                .Where(userDay => userDay.UserId.Equals(request.UserId))
                .Where(userDay => userDay.Water > 0)
                .Where(userDay => userDay.Day >= request.StartDate);
                
            if(request.EndDate.HasValue)
            {
                exp = exp.Where(userDay => userDay.Day <= request.EndDate);
            }

            return exp.AsNoTracking()
                .Select(userDay => new GraphValue(userDay.Water, userDay.Day))
                .AsAsyncEnumerable();
        }
    }
}