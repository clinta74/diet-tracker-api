using System;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.Fuelings
{
    public record DeleteFueling(int FuelingId) : IRequest<bool>;

    public class DeleteFuelingHandler : IRequestHandler<DeleteFueling, bool>
    {
        private readonly DietTrackerDbContext _dbContext;
        public DeleteFuelingHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> Handle(DeleteFueling request, CancellationToken cancellationToken)
        {
            var data = await _dbContext.Fuelings
                .AsNoTracking()
                .SingleOrDefaultAsync(fueling => fueling.FuelingId == request.FuelingId, cancellationToken);

            if (data == null)
            {
                throw new ArgumentException($"Fueling Id ({request.FuelingId}) not found.");
            }
            
            _dbContext.Fuelings.Remove(data);

            return await _dbContext.SaveChangesAsync(cancellationToken) == 1;
        }
    }
}