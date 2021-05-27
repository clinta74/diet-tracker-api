using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using diet_tracker_api.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.CQRS.UserDailyTrackingValues
{
    public record UpdateUserDailyTrackingValue(DateTime Day, string UserId, int UserTrackingValueId, int Occurance, int Value, DateTime When) : IRequest<bool>;
    public class UpdateUserDailyTrackingValueHandler : IRequestHandler<UpdateUserDailyTrackingValue, bool>
    {
        private readonly DietTrackerDbContext _dbContext;

        public UpdateUserDailyTrackingValueHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> Handle(UpdateUserDailyTrackingValue request, CancellationToken cancellationToken)
        {
            var data = await _dbContext.UserDailyTrackingValues
                        .AsNoTracking()
                        .Where(u => u.Day.Equals(request.Day))
                        .Where(u => u.UserId.Equals(request.UserId))
                        .Where(u => u.UserTrackingValueId.Equals(request.UserTrackingValueId))
                        .Where(u => u.Occurrence.Equals(request.Occurance))
                        .SingleOrDefaultAsync(cancellationToken);

            if (data == null) return false;

            var result = _dbContext.UserDailyTrackingValues
                .Update(data with
                {
                    Value = request.Value,
                    When = request.When,
                });

            return await _dbContext.SaveChangesAsync(cancellationToken) == 1;
        }
    }
}