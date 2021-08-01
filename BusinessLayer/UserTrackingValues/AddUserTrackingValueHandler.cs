using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using MediatR;

namespace diet_tracker_api.BusinessLayer.UserTrackingValues
{
    public record AddUserTrackingValue(int UserTrackingId, string Name, string Description, int Order, UserTrackingType Type, bool Disabled, IEnumerable<UserTrackingValueMetadata> Metadata) : IRequest<int>;
    public class AddUserTrackingValueHandler : IRequestHandler<AddUserTrackingValue, int>
    {
        private readonly DietTrackerDbContext _dbContext;

        public AddUserTrackingValueHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<int> Handle(AddUserTrackingValue request, CancellationToken cancellationToken)
        {
            var data = _dbContext.UserTrackingValues
                .Add(new UserTrackingValue
                {
                    UserTrackingId = request.UserTrackingId,
                    Name = request.Name,
                    Description = request.Description,
                    Order = request.Order,
                    Type = request.Type,
                    Disabled = request.Disabled,
                    Metadata = request.Metadata.ToArray(),
                });

            await _dbContext.SaveChangesAsync(cancellationToken);

            return data.Entity.UserTrackingValueId;
        }
    }
}