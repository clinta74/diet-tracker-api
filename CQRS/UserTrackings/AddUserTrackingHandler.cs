using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using MediatR;

namespace diet_tracker_api.CQRS.UserTrackings
{
    public record AddUserTracking(string UserId, string Title, string Description, int Occurrences, int Order) : IRequest<int>;
    public class AddUserTrackingHandler : IRequestHandler<AddUserTracking, int>
    {
        private readonly DietTrackerDbContext _dbContext;

        public AddUserTrackingHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> Handle(AddUserTracking request, CancellationToken cancellationToken)
        {
            var result = _dbContext.UserTrackings
               .Add(new UserTracking
               {
                   UserId = request.UserId,
                   Title = request.Title,
                   Description = request.Description,
                   Disabled = false,
                   Occurrences = request.Occurrences,
                   Order = request.Order,
               });

            await _dbContext.SaveChangesAsync();

            return result.Entity.UserTrackingId;
        }
    }
}