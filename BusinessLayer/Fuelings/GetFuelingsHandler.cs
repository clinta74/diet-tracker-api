using System.Collections.Generic;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.Fuelings
{
    public record GetFuelings() : IRequest<IAsyncEnumerable<Fueling>>;
    
    public class GetFuelingsHandler : RequestHandler<GetFuelings, IAsyncEnumerable<Fueling>>
    {
        private readonly DietTrackerDbContext _dbContext;
        public GetFuelingsHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        protected override IAsyncEnumerable<Fueling> Handle(GetFuelings request)
        {
            return _dbContext.Fuelings
                .AsNoTracking()
                .AsAsyncEnumerable();
        }
    }
}