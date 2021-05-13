using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.CQRS.UserTrackings
{
    public record UpdateUserTracking(int UserTrackingId, bool Removed, string Name, string Description, int Occurance, UserTrackingType Type) : IRequest<bool>;
    public class UpdateUserTrackingHandler : IRequestHandler<UpdateUserTracking, bool>
    {
        private readonly DietTrackerDbContext _dbContext;

        public UpdateUserTrackingHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> Handle(UpdateUserTracking request, CancellationToken cancellationToken)
        {
            var data = await _dbContext.UserTrackings
                        .AsNoTracking()
                        .Where(u => u.UserTrackingId.Equals(request.UserTrackingId))
                        .SingleOrDefaultAsync(cancellationToken);

            if (data == null) return false;

            _dbContext.UserTrackings
                .Update(data with
                {
                    Name = request.Name,
                    Description = request.Description,
                    Removed = request.Removed,
                    Occurances = request.Occurance,
                    Type = request.Type
                });

            return await _dbContext.SaveChangesAsync(cancellationToken) == 1;
        }
    }
}