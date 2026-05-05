using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.Account
{
    public record SetUserCredentials(string UserId, string Email, string PasswordHash) : IRequest<Unit>;

    public class SetUserCredentialsHandler : IRequestHandler<SetUserCredentials, Unit>
    {
        private readonly DietTrackerDbContext _dbContext;

        public SetUserCredentialsHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async ValueTask<Unit> Handle(SetUserCredentials request, CancellationToken cancellationToken)
        {
            var normalizedEmail = request.Email.ToLowerInvariant();

            var existing = await _dbContext.UserCredentials
                .FirstOrDefaultAsync(c => c.UserId == request.UserId, cancellationToken);

            if (existing != null)
            {
                _dbContext.Entry(existing).CurrentValues.SetValues(
                    existing with { Email = normalizedEmail, PasswordHash = request.PasswordHash });
            }
            else
            {
                _dbContext.UserCredentials.Add(new DataLayer.Models.UserCredentials
                {
                    UserId = request.UserId,
                    Email = normalizedEmail,
                    PasswordHash = request.PasswordHash,
                });
            }

            await _dbContext.SaveChangesAsync(cancellationToken);
            return Unit.Value;
        }
    }
}
