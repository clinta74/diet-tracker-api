using diet_tracker_api.DataLayer;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.Auth;

public record RevokeAllRefreshTokens(string UserId) : IRequest<Unit>;

public class RevokeAllRefreshTokensHandler(DietTrackerDbContext dbContext) : IRequestHandler<RevokeAllRefreshTokens, Unit>
{
    private readonly DietTrackerDbContext _dbContext = dbContext;

    public async ValueTask<Unit> Handle(RevokeAllRefreshTokens request, CancellationToken cancellationToken)
    {
        var tokens = await _dbContext.RefreshTokens
            .Where(rt => rt.UserId == request.UserId && rt.RevokedAt == null)
            .ToListAsync(cancellationToken);

        var now = DateTime.UtcNow;
        foreach (var token in tokens)
        {
            _dbContext.Entry(token).CurrentValues.SetValues(token with { RevokedAt = now });
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
