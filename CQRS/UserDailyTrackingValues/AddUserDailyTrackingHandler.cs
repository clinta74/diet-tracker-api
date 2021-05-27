using System;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using MediatR;

namespace diet_tracker_api.CQRS.UserDailyTrackingValues
{
    public record AddUserDailyTracking(DateTime Day, string UserId, int UserTrackingValueId, int Occurance, int Value, DateTime When) : IRequest<bool>;

    public class AddUserDailyTrackingHandler: IRequestHandler<AddUserDailyTracking, bool>
    {
        private readonly DietTrackerDbContext _dbContext;

        public AddUserDailyTrackingHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> Handle(AddUserDailyTracking request, CancellationToken cancellationToken)
        {
            var result = _dbContext.UserDailyTrackingValues
                .Add(new UserDailyTrackingValue
                {
                    Day = request.Day,
                    UserId = request.UserId,
                    UserTrackingValueId = request.UserTrackingValueId,
                    Occurrence = request.Occurance,
                    Value = request.Value,
                    When = request.When,
                });
            
            return await _dbContext.SaveChangesAsync(cancellationToken) == 1;
        }
    }
}