using diet_tracker_api.DataLayer;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.Admin;

public record GetUserPermissions(string UserId) : IRequest<IReadOnlyList<string>>;

public class GetUserPermissionsHandler(DietTrackerDbContext dbContext) : IRequestHandler<GetUserPermissions, IReadOnlyList<string>>
{
    private readonly DietTrackerDbContext _dbContext = dbContext;

    public async ValueTask<IReadOnlyList<string>> Handle(GetUserPermissions request, CancellationToken cancellationToken)
    {
        return await _dbContext.UserPermissions
            .AsNoTracking()
            .Where(p => p.UserId == request.UserId)
            .Select(p => p.Permission)
            .ToListAsync(cancellationToken);
    }
}
