using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.Users
{
    public record UserExists(string UserId) : IRequest<bool>;
    public class UserExistsHandler : IRequestHandler<UserExists, bool>
    {
        private readonly DietTrackerDbContext _dbContext;
        public UserExistsHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> Handle(UserExists request, CancellationToken cancellationToken)
        {
            return await _dbContext
                .Users
                .AsNoTracking()
                .AnyAsync(user => user.UserId == request.UserId);
        }
    }
}