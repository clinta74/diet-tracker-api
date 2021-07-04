using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.CQRS.UserDailyTrackingValues
{
    public record UpdateUserDailyTrackingValue(int UserTrackingValueId, int Occurance, decimal Value, Nullable<DateTime> When);
    public record UpdateUserDailyTrackingValues(DateTime Day, string UserId, UpdateUserDailyTrackingValue[] Values) : IRequest<IEnumerable<UserDailyTrackingValue>>;
    public class UpdateUserDailyTrackingValueHandler : IRequestHandler<UpdateUserDailyTrackingValues, IEnumerable<UserDailyTrackingValue>>
    {
        private readonly DietTrackerDbContext _dbContext;

        public UpdateUserDailyTrackingValueHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<IEnumerable<UserDailyTrackingValue>> Handle(UpdateUserDailyTrackingValues request, CancellationToken cancellationToken)
        {

            var results = new List<UserDailyTrackingValue>();
            using var transaction = _dbContext.Database.BeginTransaction();

            foreach (var value in request.Values)
            {
                var data = await _dbContext.UserDailyTrackingValues
                            .AsNoTracking()
                            .Where(u => u.Day.Equals(request.Day))
                            .Where(u => u.UserId.Equals(request.UserId))
                            .Where(u => u.UserTrackingValueId.Equals(value.UserTrackingValueId))
                            .Where(u => u.Occurrence.Equals(value.Occurance))
                            .SingleOrDefaultAsync(cancellationToken);

                if (data == null)
                {
                    var result = _dbContext.UserDailyTrackingValues
                    .Add(new UserDailyTrackingValue
                    {
                        Day = request.Day,
                        UserId = request.UserId,
                        UserTrackingValueId = value.UserTrackingValueId,
                        Occurrence = value.Occurance,
                        Value = value.Value,
                        When = value.When,
                    });

                    await _dbContext.SaveChangesAsync(cancellationToken);

                    results.Add(result.Entity);
                }
                else
                {
                    var result = _dbContext.UserDailyTrackingValues
                        .Update(data with
                        {
                            Value = value.Value,
                            When = value.When,
                        });

                    await _dbContext.SaveChangesAsync(cancellationToken);

                    results.Add(result.Entity);
                }
            }
            await transaction.CommitAsync(cancellationToken);

            return results;
        }
    }
}