using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace diet_tracker_api.BusinessLayer.Victories
{
    public record GetVictories(string UserId, Nullable<VictoryType> Type, Nullable<DateTime> When) : IRequest<IEnumerable<Victory>>;
    
    public class GetVictoriesHandler : IRequestHandler<GetVictories, IEnumerable<Victory>>
    {
        private readonly DietTrackerDbContext _dbContext;
        public GetVictoriesHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<IEnumerable<DataLayer.Models.Victory>> Handle(GetVictories request, CancellationToken cancellationToken)
        {
            var exp = _dbContext.Victories
                .Where(victory => victory.UserId == request.UserId);

            if (request.Type.HasValue)
            {
                exp = exp.Where(victory => victory.Type == request.Type.Value);
            }

            if (request.When.HasValue)
            {
                exp = exp.Where(victory => victory.When == request.When.Value);
            }

            return await exp
                .AsNoTracking()
                .OrderBy(victory => victory.When)
                .ToListAsync(cancellationToken);

        }
    }
}