using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.Account;

public record GetCredentialsByUserId(string UserId) : IRequest<UserCredentials?>;

public class GetCredentialsByUserIdHandler(DietTrackerDbContext dbContext) : IRequestHandler<GetCredentialsByUserId, UserCredentials?>
{
    private readonly DietTrackerDbContext _dbContext = dbContext;

    public async ValueTask<UserCredentials?> Handle(GetCredentialsByUserId request, CancellationToken cancellationToken)
    {
        return await _dbContext.UserCredentials
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.UserId == request.UserId, cancellationToken);
    }
}
