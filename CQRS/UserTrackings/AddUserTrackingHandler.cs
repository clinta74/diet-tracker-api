using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using MediatR;

namespace diet_tracker_api.CQRS.UserTrackings
{
    public record AddUserTracking(string Name, string Description, int Occurance, UserTrackingType Type) : IRequest<UserTracking>;
    public class AddUserTrackingHandler : IRequestHandler<AddUserTracking, UserTracking>
    {
        private readonly DietTrackerDbContext _dbContext;

        public AddUserTrackingHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<UserTracking> Handle(AddUserTracking request, CancellationToken cancellationToken)
        {
            var result = _dbContext.UserTrackings
               .Add(new UserTracking
               {
                   Name = request.Name,
                   Description = request.Description,
                   Removed = false,
                   Occurances = request.Occurance,
                   Type = request.Type
               });

            await _dbContext.SaveChangesAsync();

            return result.Entity;
        }
    }
}