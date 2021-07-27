using System;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.CQRS.Fuelings
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
            var data = await _dbContext.Fuelings
                .AsNoTracking()
                .SingleOrDefaultAsync(fueling => fueling.FuelingId == request.FuelingId, cancellationToken);

            if (data == null)
            {
                throw new ArgumentException($"Fueling Id ({request.FuelingId}) not found.");
            }

            _dbContext.Fuelings.Update(data with {
                Name = request.Name
            });

            return await _dbContext.SaveChangesAsync(cancellationToken) == 1;
        }
    }
}