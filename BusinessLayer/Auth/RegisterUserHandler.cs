using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.Auth
{
    public record RegisterUser(
        string FirstName,
        string LastName,
        string Email,
        string PasswordHash,
        int PlanId
    ) : IRequest<RegisterUserResult>;

    public record RegisterUserResult(string UserId, IReadOnlyList<string> Permissions);

    public class RegisterUserHandler : IRequestHandler<RegisterUser, RegisterUserResult>
    {
        private static readonly string[] DefaultPermissions = new[]
        {
            "write:user", "write:fuelings", "write:plans", "write:lean-and-greens"
        };

        private readonly DietTrackerDbContext _dbContext;

        public RegisterUserHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async ValueTask<RegisterUserResult> Handle(RegisterUser request, CancellationToken cancellationToken)
        {
            var isFirstUser = !await _dbContext.Users.AnyAsync(cancellationToken);

            var userId = Guid.NewGuid().ToString();

            var permissions = isFirstUser
                ? DefaultPermissions.Concat(new[] { "admin:users" }).ToArray()
                : DefaultPermissions;

            var userPlans = new[] { new UserPlan { PlanId = request.PlanId, Start = DateTime.Now } };
            var userPermissions = permissions.Select(p => new UserPermission { UserId = userId, Permission = p }).ToList();

            _dbContext.Users.Add(new User
            {
                UserId = userId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                EmailAddress = request.Email,
                Created = DateTime.Now,
                UserPlans = userPlans,
            });

            _dbContext.UserCredentials.Add(new UserCredentials
            {
                UserId = userId,
                Email = request.Email.ToLowerInvariant(),
                PasswordHash = request.PasswordHash,
            });

            _dbContext.UserPermissions.AddRange(userPermissions);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return new RegisterUserResult(userId, permissions);
        }
    }
}
