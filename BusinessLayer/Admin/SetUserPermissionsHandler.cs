using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.Admin
{
    public record SetUserPermissions(string UserId, IReadOnlyList<string> Permissions) : IRequest<Unit>;

    public class SetUserPermissionsHandler : IRequestHandler<SetUserPermissions, Unit>
    {
        private readonly DietTrackerDbContext _dbContext;

        public SetUserPermissionsHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async ValueTask<Unit> Handle(SetUserPermissions request, CancellationToken cancellationToken)
        {
            var existing = await _dbContext.UserPermissions
                .Where(p => p.UserId == request.UserId)
                .ToListAsync(cancellationToken);

            _dbContext.UserPermissions.RemoveRange(existing);

            var newPermissions = request.Permissions.Select(p => new UserPermission
            {
                UserId = request.UserId,
                Permission = p
            });

            _dbContext.UserPermissions.AddRange(newPermissions);

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}
