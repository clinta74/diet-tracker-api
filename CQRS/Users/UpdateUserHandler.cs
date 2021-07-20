using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.CQRS.Users
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
            var user = _dbContext.Users
                .AsNoTracking()
                .FirstOrDefault(user => user.UserId.Equals(request.userId));

            if (user != null)
            {
                _dbContext.Users
                .Update(user with
                {
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    WaterSize = request.WaterSize,
                    WaterTarget = request.WaterTarget,
                    Autosave = request.Autosave,
                });

                return await _dbContext.SaveChangesAsync() == 1;
            }

            return false;
        }
    }
}