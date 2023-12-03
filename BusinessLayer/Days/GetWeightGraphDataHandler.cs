using System.Collections.Generic;
using System.Linq;
using System.Threading;
using diet_tracker_api.DataLayer;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.Days
{
    public record GetWeightGraphData(string UserId, DateTime StartDate, DateTime? EndDate) :  
        GetGraphData(UserId, StartDate, EndDate), IStreamRequest<GraphValue>;
    public class GetWeightGraphDataHandler : IStreamRequestHandler<GetWeightGraphData, GraphValue>
    {
        private readonly DietTrackerDbContext _dbContext;

        public GetWeightGraphDataHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IAsyncEnumerable<GraphValue> Handle(GetWeightGraphData request, CancellationToken cancellationToken)
        {
            var exp = _dbContext.UserDays
                .Where(userDay => userDay.UserId.Equals(request.UserId))
                .Where(userDay => userDay.Weight > 0)
                .Where(userDay => userDay.Day >= request.StartDate);
                
            if(request.EndDate.HasValue)
            {
                exp = exp.Where(userDay => userDay.Day <= request.EndDate);
            }

            return exp.AsNoTracking()
                .Select(userDay => new GraphValue(userDay.Weight, userDay.Day))
                .AsAsyncEnumerable();
        }
    }
}