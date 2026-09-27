using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.Auth;

public record RotateRefreshToken(
    string OldTokenHash,
    string NewTokenHash,
    DateTime NewExpiresAt,
    string? CreatedByIp
) : IRequest<RotateRefreshTokenResult?>;

public record RotateRefreshTokenResult(string UserId, int NewTokenId);

public class RotateRefreshTokenHandler(DietTrackerDbContext dbContext) : IRequestHandler<RotateRefreshToken, RotateRefreshTokenResult?>
{
    private readonly DietTrackerDbContext _dbContext = dbContext;

    public async ValueTask<RotateRefreshTokenResult?> Handle(RotateRefreshToken request, CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;

        var old = await _dbContext.RefreshTokens
            .AsNoTracking()
            .Where(rt => rt.TokenHash == request.OldTokenHash && rt.RevokedAt == null && rt.ExpiresAt > now)
            .Select(rt => new { rt.Id, rt.UserId })
            .FirstOrDefaultAsync(cancellationToken);

        if (old == null) return null;

        await using var transaction = await _dbContext.Database.BeginTransactionAsync(cancellationToken);

        // Revoking is the gate: a single conditional UPDATE, so of several concurrent requests presenting
        // the same token only one can still see RevokedAt == null. The others wait on the row lock, then
        // match nothing and are rejected, which keeps each refresh token single-use.
        var revoked = await _dbContext.RefreshTokens
            .Where(rt => rt.Id == old.Id && rt.RevokedAt == null)
            .ExecuteUpdateAsync(setters => setters.SetProperty(rt => rt.RevokedAt, now), cancellationToken);

        if (revoked == 0) return null;

        var newEntry = _dbContext.RefreshTokens.Add(new RefreshToken
        {
            UserId = old.UserId,
            TokenHash = request.NewTokenHash,
            ExpiresAt = request.NewExpiresAt,
            CreatedAt = now,
            CreatedByIp = request.CreatedByIp,
        });

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _dbContext.RefreshTokens
            .Where(rt => rt.Id == old.Id)
            .ExecuteUpdateAsync(setters => setters.SetProperty(rt => rt.ReplacedByTokenId, newEntry.Entity.Id), cancellationToken);

        await transaction.CommitAsync(cancellationToken);

        return new RotateRefreshTokenResult(old.UserId, newEntry.Entity.Id);
    }
}
