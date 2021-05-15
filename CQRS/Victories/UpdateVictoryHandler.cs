using System;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.CQRS.Victories
{
    public record UpdateVictory(int VictoryId, string UserId, string Name, DateTime? When, VictoryType Type) : IRequest<bool>;

public class UpdateVictoryHandler : IRequestHandler<UpdateVictory, bool>
    {
        private readonly DietTrackerDbContext ctx;
        public UpdateVictoryHandler(DietTrackerDbContext context)
        {
            ctx = context;
        }

        public async Task<bool> Handle(UpdateVictory request, CancellationToken cancellationToken)
        {
            var data = await ctx.Victories
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.VictoryId == request.VictoryId, cancellationToken);

            if (data == null) return false;

            ctx.Victories.Update(data with
            {
                Name = request.Name,
                When = request.When,
            });

            await ctx.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}