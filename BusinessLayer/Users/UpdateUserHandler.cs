using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.Users
{
    public record UpdateUser(string userId, string FirstName, string LastName, int WaterSize, int WaterTarget, bool Autosave) : IRequest<bool>;
    public class UpdateUserHandler : IRequestHandler<UpdateUser, bool>
    {
        private readonly DietTrackerDbContext _dbContext;
        public UpdateUserHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<bool> Handle(UpdateUser request, CancellationToken cancellationToken)
        {
            var rowsAffected = await _dbContext.Users
                .Where(user => user.UserId.Equals(request.userId))
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(u => u.FirstName, request.FirstName)
                    .SetProperty(u => u.LastName, request.LastName)
                    .SetProperty(u => u.WaterSize, request.WaterSize)
                    .SetProperty(u => u.WaterTarget, request.WaterTarget)
                    .SetProperty(u => u.Autosave, request.Autosave),
                    cancellationToken);

            return rowsAffected == 1;
        }
    }
}