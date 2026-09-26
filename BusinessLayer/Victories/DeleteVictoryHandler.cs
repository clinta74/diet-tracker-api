using diet_tracker_api.DataLayer;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.Victories;

public record DeleteVictory(int VictoryId) : IRequest<bool>;

public class DeleteVictoryHandler(DietTrackerDbContext context) : IRequestHandler<DeleteVictory, bool>
{
    private readonly DietTrackerDbContext ctx = context;

    public async ValueTask<bool> Handle(DeleteVictory request, CancellationToken cancellationToken)
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