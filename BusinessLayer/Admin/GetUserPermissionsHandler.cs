using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.Admin
{
    public record GetUserPermissions(string UserId) : IRequest<IReadOnlyList<string>>;

    public class GetUserPermissionsHandler : IRequestHandler<GetUserPermissions, IReadOnlyList<string>>
    {
        private readonly DietTrackerDbContext _dbContext;

        public GetUserPermissionsHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async ValueTask<IReadOnlyList<string>> Handle(GetUserPermissions request, CancellationToken cancellationToken)
        {
            return await _dbContext.UserPermissions
                .AsNoTracking()
                .Where(p => p.UserId == request.UserId)
                .Select(p => p.Permission)
                .ToListAsync(cancellationToken);
        }
    }
}
