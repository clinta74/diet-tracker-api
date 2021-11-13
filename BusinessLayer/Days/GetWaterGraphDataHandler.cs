using System;
using System.Collections.Generic;
using System.Linq;
using diet_tracker_api.DataLayer;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.Days
{
    public record GetWaterGraphData(string UserId, DateOnly StartDate, Nullable<DateOnly> EndDate) : 
    GetGraphData(UserId, StartDate, EndDate), IRequest<IAsyncEnumerable<GraphValue>>;
    public class GetWaterGraphDataHandler : RequestHandler<GetWaterGraphData, IAsyncEnumerable<GraphValue>>
    {
        private readonly DietTrackerDbContext _dbContext;

        public GetWaterGraphDataHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override IAsyncEnumerable<GraphValue> Handle(GetWaterGraphData request)
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