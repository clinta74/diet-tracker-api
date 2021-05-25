using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.CQRS.UserTrackings
{
    public record UpdateUserTracking(int UserTrackingId, string Name, string Description, int Occurrences) : IRequest<bool>;
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
                    Title = request.Name,
                    Description = request.Description,
                    Occurrences = request.Occurrences,
                    Removed = false
                });

            return await _dbContext.SaveChangesAsync(cancellationToken) == 1;
        }
    }
}