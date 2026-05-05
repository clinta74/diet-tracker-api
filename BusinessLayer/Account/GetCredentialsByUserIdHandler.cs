#nullable enable
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.Account
{
    public record GetCredentialsByUserId(string UserId) : IRequest<UserCredentials?>;

    public class GetCredentialsByUserIdHandler : IRequestHandler<GetCredentialsByUserId, UserCredentials?>
    {
        private readonly DietTrackerDbContext _dbContext;

        public GetCredentialsByUserIdHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async ValueTask<UserCredentials?> Handle(GetCredentialsByUserId request, CancellationToken cancellationToken)
        {
            return await _dbContext.UserCredentials
                .AsNoTracking()
                .FirstOrDefaultAsync(c => c.UserId == request.UserId, cancellationToken);
        }
    }
}
