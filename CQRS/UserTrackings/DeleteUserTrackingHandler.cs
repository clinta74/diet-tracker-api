using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.CQRS.UserTrackings
{
    public record DeleteUserTracking(int UserTrackingId) : IRequest<bool>;
    public class DeleteUserTrackingHandler : IRequestHandler<DeleteUserTracking, bool>
    {
        private readonly DietTrackerDbContext _dbContext;

        public DeleteUserTrackingHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<bool> Handle(DeleteUserTracking request, CancellationToken cancellationToken)
        {
            var data = await _dbContext.UserTrackings
                        .AsNoTracking()
                        .Where(u => u.UserTrackingId.Equals(request.UserTrackingId))
                        .SingleOrDefaultAsync(cancellationToken);

            if (data == null) return false;

            _dbContext.UserTrackings
                .Update(data with
                {
                    Removed = true,
                });

            return await _dbContext.SaveChangesAsync(cancellationToken) == 1;
        }
    }
}