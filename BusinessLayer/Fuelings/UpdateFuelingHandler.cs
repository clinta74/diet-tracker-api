using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.Fuelings
{
    public record UpdateFueling(int FuelingId, string Name) : IRequest<bool>;

    public class UpdateFuelingHandler : IRequestHandler<UpdateFueling, bool>
    {
        private readonly DietTrackerDbContext _dbContext;
        public UpdateFuelingHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> Handle(UpdateFueling request, CancellationToken cancellationToken)
        {
            var rowsAffected = await _dbContext.Fuelings
                .Where(fueling => fueling.FuelingId == request.FuelingId)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(f => f.Name, request.Name),
                    cancellationToken);

            if (rowsAffected == 0)
            {
                throw new ArgumentException($"Fueling Id ({request.FuelingId}) not found.");
            }

            return rowsAffected == 1;
        }
    }
}