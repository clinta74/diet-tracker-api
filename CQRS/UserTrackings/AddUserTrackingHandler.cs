using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.CQRS.UserTrackings
{
    public record AddUserTracking(string UserId, string Title, string Description, int Occurrences, int Order, IEnumerable<UserTrackingValue> Values) : IRequest<UserTracking>;
    public class AddUserTrackingHandler : IRequestHandler<AddUserTracking, UserTracking>
    {
        private readonly DietTrackerDbContext _dbContext;

        public AddUserTrackingHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<UserTracking> Handle(AddUserTracking request, CancellationToken cancellationToken)
        {
            var order = await _dbContext.UserTrackings
                .Where(t => t.UserId.Equals(request.UserId))
                .OrderByDescending(t => t.Order)
                .Select(t => t.Order)
                .FirstOrDefaultAsync(cancellationToken);

            var result = _dbContext.UserTrackings
               .Add(new UserTracking
               {
                   UserId = request.UserId,
                   Title = request.Title,
                   Description = request.Description,
                   Disabled = false,
                   Occurrences = request.Occurrences,
                   Order = order + 1,
                   Values = request.Values.ToList(),
               });

            await _dbContext.SaveChangesAsync(cancellationToken);

            return result.Entity;
        }
    }
}