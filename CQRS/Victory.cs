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
    public record GetVictories(string UserId, VictoryType Type) : IRequest<IEnumerable<Victory>>;
    public record AddVictory(string UserId, string Name, DateTime When, VictoryType Type) : IRequest<Victory>;
    public record UpdateVictory(int VictoryId, string UserId, string Name, DateTime When, VictoryType Type) : IRequest<Victory>;
    public record DeleteVictory(int VictoryId) : IRequest<Unit>;

    public class GetVictoriesHandler : IRequestHandler<GetVictories, IEnumerable<Victory>>
    {
        private readonly DietTrackerDbContext ctx;
        public GetVictoriesHandler(DietTrackerDbContext context)
        {
            ctx = context;
        }
        public async Task<IEnumerable<DataLayer.Models.Victory>> Handle(GetVictories request, CancellationToken cancellationToken)
        {
            return await ctx.Victories
                .Where(victory => victory.UserId == request.UserId)
                .AsNoTracking()
                .ToListAsync();
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

            await ctx.SaveChangesAsync();

            return data.Entity;
        }
    }
}