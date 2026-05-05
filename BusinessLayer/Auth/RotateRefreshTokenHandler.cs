#nullable enable
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.Auth
{
    public record RotateRefreshToken(
        string OldTokenHash,
        string NewTokenHash,
        DateTime NewExpiresAt,
        string? CreatedByIp
    ) : IRequest<RotateRefreshTokenResult?>;

    public record RotateRefreshTokenResult(string UserId, int NewTokenId);

    public class RotateRefreshTokenHandler : IRequestHandler<RotateRefreshToken, RotateRefreshTokenResult?>
    {
        private readonly DietTrackerDbContext _dbContext;

        public RotateRefreshTokenHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async ValueTask<RotateRefreshTokenResult?> Handle(RotateRefreshToken request, CancellationToken cancellationToken)
        {
            var old = await _dbContext.RefreshTokens
                .FirstOrDefaultAsync(rt =>
                    rt.TokenHash == request.OldTokenHash &&
                    rt.RevokedAt == null &&
                    rt.ExpiresAt > DateTime.UtcNow,
                    cancellationToken);

            if (old == null) return null;

            var newEntry = _dbContext.RefreshTokens.Add(new RefreshToken
            {
                UserId = old.UserId,
                TokenHash = request.NewTokenHash,
                ExpiresAt = request.NewExpiresAt,
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = request.CreatedByIp,
            });

            await _dbContext.SaveChangesAsync(cancellationToken);

            var oldToUpdate = await _dbContext.RefreshTokens.FindAsync(new object[] { old.Id }, cancellationToken);
            _dbContext.Entry(oldToUpdate!).CurrentValues.SetValues(oldToUpdate! with
            {
                RevokedAt = DateTime.UtcNow,
                ReplacedByTokenId = newEntry.Entity.Id,
            });

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new RotateRefreshTokenResult(old.UserId, newEntry.Entity.Id);
        }
    }
}
