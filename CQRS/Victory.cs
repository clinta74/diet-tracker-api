using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.CQRS
{
    public record GetVictories(string UserId, Nullable<VictoryType> Type) : IRequest<IEnumerable<Victory>>;
    public record AddVictory(string UserId, string Name, DateTime? When, VictoryType Type) : IRequest<Victory>;
    public record UpdateVictory(int VictoryId, string UserId, string Name, DateTime? When, VictoryType Type) : IRequest<bool>;
    public record DeleteVictory(int VictoryId) : IRequest<bool>;

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

    public class AddVictoryHandler : IRequestHandler<AddVictory, Victory>
    {
        private readonly DietTrackerDbContext ctx;
        public AddVictoryHandler(DietTrackerDbContext context)
        {
            ctx = context;
        }

        public async Task<Victory> Handle(AddVictory request, CancellationToken cancellationToken)
        {
            var data = ctx.Add(new Victory
            {
                UserId = request.UserId,
                Name = request.Name,
                When = request.When,
                Type = request.Type,
            });

            await ctx.SaveChangesAsync(cancellationToken);

            return data.Entity;
        }
    }

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

            ctx.Victories.Update(data with {
                Name = request.Name,
                When = request.When,
            });

            await ctx.SaveChangesAsync(cancellationToken);

            return true;
        }
    }

    public class DeleteVictoryHandler : IRequestHandler<DeleteVictory, bool>
    {
        private readonly DietTrackerDbContext ctx;
        public DeleteVictoryHandler(DietTrackerDbContext context)
        {
            ctx = context;
        }

        public async Task<bool> Handle(DeleteVictory request, CancellationToken cancellationToken)
        {
            var data = await ctx.Victories
                .AsNoTracking()
                .FirstOrDefaultAsync(v => v.VictoryId == request.VictoryId, cancellationToken);

            if (data == null) return false;

            ctx.Victories.Remove(data);

            await ctx.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}