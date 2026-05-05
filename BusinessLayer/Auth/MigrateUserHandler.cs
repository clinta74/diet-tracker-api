#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.Auth
{
    public record MigrateUser(string Email, string PasswordHash) : IRequest<MigrateUserResult?>;

    public record MigrateUserResult(string? UserId, IReadOnlyList<string>? Permissions, string? Error);

    public class MigrateUserHandler : IRequestHandler<MigrateUser, MigrateUserResult?>
    {
        private static readonly string[] DefaultPermissions = new[]
        {
            "write:user", "write:fuelings", "write:plans", "write:lean-and-greens"
        };

        private readonly DietTrackerDbContext _dbContext;

        public MigrateUserHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async ValueTask<MigrateUserResult?> Handle(MigrateUser request, CancellationToken cancellationToken)
        {
            var normalizedEmail = request.Email.ToLowerInvariant();

            var user = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.EmailAddress.ToLower() == normalizedEmail, cancellationToken);

            if (user == null)
            {
                return new MigrateUserResult(null, null, "No account found with that email address.");
            }

            var existingCredentials = await _dbContext.UserCredentials
                .AsNoTracking()
                .AnyAsync(c => c.UserId == user.UserId, cancellationToken);

            if (existingCredentials)
            {
                return new MigrateUserResult(null, null, "This account has already been migrated. Please log in normally.");
            }

            _dbContext.UserCredentials.Add(new DataLayer.Models.UserCredentials
            {
                UserId = user.UserId,
                Email = normalizedEmail,
                PasswordHash = request.PasswordHash,
            });

            var permissions = DefaultPermissions;
            var userPermissions = permissions.Select(p => new DataLayer.Models.UserPermission
            {
                UserId = user.UserId,
                Permission = p
            }).ToList();

            _dbContext.UserPermissions.AddRange(userPermissions);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new MigrateUserResult(user.UserId, permissions, null);
        }
    }
}
