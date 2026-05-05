using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.Auth
{
    public record RevokeRefreshTokenById(int TokenId, string UserId) : IRequest<bool>;

    public class RevokeRefreshTokenByIdHandler : IRequestHandler<RevokeRefreshTokenById, bool>
    {
        private readonly DietTrackerDbContext _dbContext;

        public RevokeRefreshTokenByIdHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async ValueTask<bool> Handle(RevokeRefreshTokenById request, CancellationToken cancellationToken)
        {
            var token = await _dbContext.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Id == request.TokenId && rt.UserId == request.UserId, cancellationToken);

            if (token == null || token.RevokedAt != null) return false;

            _dbContext.Entry(token).CurrentValues.SetValues(token with { RevokedAt = DateTime.UtcNow });
            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
