using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using diet_tracker_api.Models;
using MediatR;

namespace diet_tracker_api.CQRS.UserDailyTrackings
{
    public record AddUserDailyTracking(DateTime Day, string UserId, int UserTrackingId, int Occurance, IEnumerable<CurrentUserDailyTrackingValueRequest> values) : IRequest<UserDailyTracking>;

    public class AddUserDailyTrackingHandler: IRequestHandler<AddUserDailyTracking, UserDailyTracking>
    {
        private readonly DietTrackerDbContext _dbContext;

        public AddUserDailyTrackingHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<UserDailyTracking> Handle(AddUserDailyTracking request, CancellationToken cancellationToken)
        {

             var values = request.values.Select(v => new UserDailyTrackingValue
            {
                Day = request.Day,
                UserId = request.UserId,
                UserTrackingId = request.UserTrackingId,
                Occurrence = request.UserTrackingId,
                Value = v.Value,
                When = v.When,
            }).ToList();

            var result = _dbContext.UserDailyTrackings
                .Add(new UserDailyTracking
                {
                    Day = request.Day,
                    UserId = request.UserId,
                    UserTrackingId = request.UserTrackingId,
                    Occurrence = request.Occurance,
                    Values = values,
                });
            
            await _dbContext.SaveChangesAsync(cancellationToken);

            return result.Entity;
        }
    }
}