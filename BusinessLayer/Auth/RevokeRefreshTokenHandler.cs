using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.Auth
{
    public record RevokeRefreshToken(string TokenHash) : IRequest<bool>;

    public class RevokeRefreshTokenHandler : IRequestHandler<RevokeRefreshToken, bool>
    {
        private readonly DietTrackerDbContext _dbContext;

        public RevokeRefreshTokenHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async ValueTask<bool> Handle(RevokeRefreshToken request, CancellationToken cancellationToken)
        {
            var token = await _dbContext.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.TokenHash == request.TokenHash, cancellationToken);

            if (token == null || token.RevokedAt != null) return false;

            _dbContext.Entry(token).CurrentValues.SetValues(token with { RevokedAt = DateTime.UtcNow });
            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
