using System;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using MediatR;

namespace diet_tracker_api.CQRS.UserDailyTrackings
{
    public record AddUserDailyTracking(DateTime Day, string UserId, int UserTrackingId, int Occurance, int Value, DateTime When) : IRequest<UserDailyTracking>;

    public class AddUserDailyTrackingHandler: IRequestHandler<AddUserDailyTracking, UserDailyTracking>
    {
        private readonly DietTrackerDbContext _dbContext;

        public AddUserDailyTrackingHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<UserDailyTracking> Handle(AddUserDailyTracking request, CancellationToken cancellationToken)
        {
            var result = _dbContext.UserDailyTrackings
                .Add(new UserDailyTracking
                {
                    Day = request.Day,
                    UserId = request.UserId,
                    UserTrackingId = request.UserTrackingId,
                    Occurrence = request.Occurance,
                    Value = request.Value,
                    When = request.When
                });
            
            await _dbContext.SaveChangesAsync(cancellationToken);

            return result.Entity;
        }
    }
}