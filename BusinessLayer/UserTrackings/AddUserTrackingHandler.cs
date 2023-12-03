using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.UserTrackings
{
    public record AddUserTracking(string UserId, string Title, string Description, int Occurrences, int Order, bool UseTime, IEnumerable<UserTrackingValue> Values) : IRequest<UserTracking>;
    public class AddUserTrackingHandler : IRequestHandler<AddUserTracking, UserTracking>
    {
        private readonly DietTrackerDbContext _dbContext;
        private readonly IMediator _mediator;

        public AddUserTrackingHandler(DietTrackerDbContext dbContext, IMediator mediator)
        {
            _dbContext = dbContext;
            _mediator = mediator;
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
                   UseTime = request.UseTime,
                   Values = request.Values.ToList(),
               });

            await _dbContext.SaveChangesAsync(cancellationToken);

            return await _mediator.Send(new GetUserTracking(request.UserId, result.Entity.UserTrackingId));
        }
    }
}