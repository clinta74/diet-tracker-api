#nullable enable
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;

namespace diet_tracker_api.BusinessLayer.Auth
{
    public record CreateRefreshToken(
        string UserId,
        string TokenHash,
        DateTime ExpiresAt,
        string? CreatedByIp
    ) : IRequest<int>;

    public class CreateRefreshTokenHandler : IRequestHandler<CreateRefreshToken, int>
    {
        private readonly DietTrackerDbContext _dbContext;

        public CreateRefreshTokenHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async ValueTask<int> Handle(CreateRefreshToken request, CancellationToken cancellationToken)
        {
            var entry = _dbContext.RefreshTokens.Add(new RefreshToken
            {
                UserId = request.UserId,
                TokenHash = request.TokenHash,
                ExpiresAt = request.ExpiresAt,
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = request.CreatedByIp,
            });

            await _dbContext.SaveChangesAsync(cancellationToken);

            return entry.Entity.Id;
        }
    }
}
