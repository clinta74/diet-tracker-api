using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.Account
{
    public record ChangePassword(string UserId, string NewPasswordHash) : IRequest<bool>;

    public class ChangePasswordHandler : IRequestHandler<ChangePassword, bool>
    {
        private readonly DietTrackerDbContext _dbContext;

        public ChangePasswordHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async ValueTask<bool> Handle(ChangePassword request, CancellationToken cancellationToken)
        {
            var credentials = await _dbContext.UserCredentials
                .FirstOrDefaultAsync(c => c.UserId == request.UserId, cancellationToken);

            if (credentials == null) return false;

            _dbContext.Entry(credentials).CurrentValues.SetValues(
                credentials with { PasswordHash = request.NewPasswordHash });

            await _dbContext.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
