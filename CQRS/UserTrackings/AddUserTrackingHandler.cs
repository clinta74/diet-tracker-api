using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using diet_tracker_api.Models;
using MediatR;

namespace diet_tracker_api.CQRS.UserTrackings
{
    public record AddUserTracking(string UserId, string Title, string Description, int Occurrences, int Order, IEnumerable<UserTrackingValueRequest> Values) : IRequest<int>;
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
                   Removed = false,
                   Occurrences = request.Occurrences,
                   Order = request.Order,
                   Values = request.Values.Select(v => new UserTrackingValue
                   {
                       Name = v.Name,
                       Description = v.Description,
                       Order = v.Order,
                       Type = v.Type,
                   }).ToList(),
               });

            await _dbContext.SaveChangesAsync();

            return result.Entity.UserTrackingId;
        }
    }
}