using System.Collections.Generic;
using System.Threading;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.Fuelings
{
    public record GetFuelings() : IStreamRequest<Fueling>;
    
    public class GetFuelingsHandler : IStreamRequestHandler<GetFuelings, Fueling>
    {
        private readonly DietTrackerDbContext _dbContext;
        public GetFuelingsHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public IAsyncEnumerable<Fueling> Handle(GetFuelings request, CancellationToken cancellationToken)
        {
            return _dbContext.Fuelings
                .AsNoTracking()
                .AsAsyncEnumerable();
        }
    }
}