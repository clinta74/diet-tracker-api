#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.Auth
{
    public record LoginUser(string Email, string Password) : IRequest<LoginUserResult?>;

    public record LoginUserResult(string UserId, IReadOnlyList<string> Permissions);

    public class LoginUserHandler : IRequestHandler<LoginUser, LoginUserResult?>
    {
        private readonly DietTrackerDbContext _dbContext;

        public LoginUserHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async ValueTask<LoginUserResult?> Handle(LoginUser request, CancellationToken cancellationToken)
        {
            var credentials = await _dbContext.UserCredentials
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Email == request.Email.ToLowerInvariant(), cancellationToken);

            if (credentials == null || !BCrypt.Net.BCrypt.Verify(request.Password, credentials.PasswordHash))
            {
                return null;
            }

            var permissions = await _dbContext.UserPermissions
                .AsNoTracking()
                .Where(p => p.UserId == credentials.UserId)
                .Select(p => p.Permission)
                .ToListAsync(cancellationToken);

            return new LoginUserResult(credentials.UserId, permissions);
        }
    }
}
