using System;
using System.Collections.Generic;
using System.Linq;
using diet_tracker_api.DataLayer;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.CQRS.Days
{
    public record GraphValue(decimal value, DateTime date);
    public record GetWeightRange(string UserId, DateTime StartDate, Nullable<DateTime> EndDate) : IRequest<IAsyncEnumerable<GraphValue>>;
    public class GetWeightRangeHandler : RequestHandler<GetWeightRange, IAsyncEnumerable<GraphValue>>
    {
        private readonly DietTrackerDbContext _dbContext;

        public GetWeightRangeHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override IAsyncEnumerable<GraphValue> Handle(GetWeightRange request)
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