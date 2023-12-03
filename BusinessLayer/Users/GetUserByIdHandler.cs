using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.Users
{
    public record GetUserById(string UserId) : IRequest<User>;

    public class GetUserByIdHandler : IRequestHandler<GetUserById, User>
    {
        private readonly DietTrackerDbContext _dbContext;
        public GetUserByIdHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<User> Handle(GetUserById request, CancellationToken cancellationToken)
        {
            return await _dbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(user => user.UserId == request.UserId);
        }
    }
}