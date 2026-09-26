using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.Victories;

public record UpdateVictory(int VictoryId, string UserId, string? Name, DateTime? When, VictoryType Type) : IRequest<bool>;

public class UpdateVictoryHandler(DietTrackerDbContext context) : IRequestHandler<UpdateVictory, bool>
{
    private readonly DietTrackerDbContext ctx = context;

    public async ValueTask<bool> Handle(UpdateVictory request, CancellationToken cancellationToken)
    {
        var rowsAffected = await ctx.Victories
            .Where(v => v.VictoryId == request.VictoryId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(v => v.Name, request.Name)
                .SetProperty(v => v.When, request.When),
                cancellationToken);

        return rowsAffected > 0;
    }
}