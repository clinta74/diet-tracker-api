using System;
using System.Collections.Generic;
using System.Linq;
using diet_tracker_api.DataLayer;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.Days
{
    public record GetWeightGraphData(string UserId, DateOnly StartDate, Nullable<DateOnly> EndDate) :  
        GetGraphData(UserId, StartDate, EndDate), IRequest<IAsyncEnumerable<GraphValue>>;
    public class GetWeightGraphDataHandler : RequestHandler<GetWeightGraphData, IAsyncEnumerable<GraphValue>>
    {
        private readonly DietTrackerDbContext _dbContext;

        public GetWeightGraphDataHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override IAsyncEnumerable<GraphValue> Handle(GetWeightGraphData request)
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