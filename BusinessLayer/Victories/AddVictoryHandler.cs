using System;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using MediatR;

namespace diet_tracker_api.BusinessLayer.Victories
{
    public record AddVictory(string UserId, string Name, DateTime? When, VictoryType Type) : IRequest<Victory>;

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
}