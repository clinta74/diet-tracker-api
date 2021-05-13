using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.CQRS.Fuelings;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.CQRS
{
    namespace Fuelings
    {
        public record GetFuelings() : IRequest<IAsyncEnumerable<Fueling>>;
    }

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