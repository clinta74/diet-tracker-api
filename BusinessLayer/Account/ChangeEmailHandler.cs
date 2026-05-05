#nullable enable
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.Account
{
    public record ChangeEmail(string UserId, string NewEmail) : IRequest<ChangeEmailResult>;

    public record ChangeEmailResult(bool Success, string? Error);

    public class ChangeEmailHandler : IRequestHandler<ChangeEmail, ChangeEmailResult>
    {
        private readonly DietTrackerDbContext _dbContext;

        public ChangeEmailHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async ValueTask<ChangeEmailResult> Handle(ChangeEmail request, CancellationToken cancellationToken)
        {
            var normalizedEmail = request.NewEmail.ToLowerInvariant();

            var taken = await _dbContext.UserCredentials
                .AnyAsync(c => c.Email == normalizedEmail && c.UserId != request.UserId, cancellationToken);

            if (taken)
                return new ChangeEmailResult(false, "Email address is already in use.");

            var credentials = await _dbContext.UserCredentials
                .FirstOrDefaultAsync(c => c.UserId == request.UserId, cancellationToken);

            if (credentials == null)
                return new ChangeEmailResult(false, "No credentials found for this user.");

            _dbContext.Entry(credentials).CurrentValues.SetValues(
                credentials with { Email = normalizedEmail });

            await _dbContext.SaveChangesAsync(cancellationToken);
            return new ChangeEmailResult(true, null);
        }
    }
}
