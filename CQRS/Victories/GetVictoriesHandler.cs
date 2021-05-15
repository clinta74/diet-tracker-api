using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
namespace diet_tracker_api.CQRS.Victories
{
    public record GetVictories(string UserId, Nullable<VictoryType> Type) : IRequest<IEnumerable<Victory>>;
    
    public class GetVictoriesHandler : IRequestHandler<GetVictories, IEnumerable<Victory>>
    {
        private readonly DietTrackerDbContext ctx;
        public GetVictoriesHandler(DietTrackerDbContext context)
        {
            ctx = context;
        }
        public async Task<IEnumerable<DataLayer.Models.Victory>> Handle(GetVictories request, CancellationToken cancellationToken)
        {
            var exp = ctx.Victories
                .Where(victory => victory.UserId == request.UserId);

            if (request.Type.HasValue)
            {
                exp.Where(victory => victory.Type == request.Type.Value);
            }
            return await exp
                .AsNoTracking()
                .ToListAsync(cancellationToken);

        }
    }
}