using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.Auth
{
    public record GetActiveRefreshTokens(string UserId) : IRequest<IReadOnlyList<RefreshToken>>;

    public class GetActiveRefreshTokensHandler : IRequestHandler<GetActiveRefreshTokens, IReadOnlyList<RefreshToken>>
    {
        private readonly DietTrackerDbContext _dbContext;

        public GetActiveRefreshTokensHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async ValueTask<IReadOnlyList<RefreshToken>> Handle(GetActiveRefreshTokens request, CancellationToken cancellationToken)
        {
            return await _dbContext.RefreshTokens
                .AsNoTracking()
                .Where(rt => rt.UserId == request.UserId && rt.RevokedAt == null && rt.ExpiresAt > DateTime.UtcNow)
                .OrderByDescending(rt => rt.CreatedAt)
                .ToListAsync(cancellationToken);
        }
    }
}
