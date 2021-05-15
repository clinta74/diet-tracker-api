using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.CQRS.Users
{
    public record UserExists(string UserId) : IRequest<bool>;
    public class UserExistsHandler : IRequestHandler<Users.UserExists, bool>
    {
        private readonly DietTrackerDbContext _dbContext;
        public UserExistsHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> Handle(UserExists request, CancellationToken cancellationToken)
        {
            var user = await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(user => user.UserId == request.UserId);

            return user != null;
        }
    }
}