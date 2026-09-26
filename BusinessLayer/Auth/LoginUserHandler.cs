#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using diet_tracker_api.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.Auth
{
    public record LoginUser(string Email, string Password) : IRequest<LoginUserResult?>;

    public record LoginUserResult(string UserId, IReadOnlyList<string> Permissions);

    public class LoginUserHandler : IRequestHandler<LoginUser, LoginUserResult?>
    {
        private readonly DietTrackerDbContext _dbContext;
        private readonly IPasswordService _passwordService;

        public LoginUserHandler(DietTrackerDbContext dbContext, IPasswordService passwordService)
        {
            _dbContext = dbContext;
            _passwordService = passwordService;
        }

        public async ValueTask<LoginUserResult?> Handle(LoginUser request, CancellationToken cancellationToken)
        {
            var credentials = await _dbContext.UserCredentials
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.Email == request.Email.ToLowerInvariant(), cancellationToken);

            var result = _passwordService.Verify(credentials?.PasswordHash, request.Password);

            if (credentials == null || result == PasswordVerificationResult.Failed)
            {
                return null;
            }

            if (result == PasswordVerificationResult.SuccessRehashNeeded)
            {
                var newHash = _passwordService.Hash(request.Password);
                await _dbContext.UserCredentials
                    .Where(c => c.UserId == credentials.UserId)
                    .ExecuteUpdateAsync(setters => setters.SetProperty(c => c.PasswordHash, newHash), cancellationToken);
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
