using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.Users
{
    public record GetCurrentUser(string UserId) : IRequest<CurrentUser>;
    public record CurrentUser
    {
        public string UserId { get; init; }
        public string FirstName { get; init; }
        public string LastName { get; init; }
        public string EmailAddress { get; init; }
        public DateTime Created { get; init; }
        public int WaterTarget { get; init; }
        public int WaterSize { get; init; }
        public Plan CurrentPlan { get; init; }
        public DateTime? Started { get; init; }
    }

    public class GetCurrentUserHandler : IRequestHandler<GetCurrentUser, CurrentUser>
    {
        private readonly DietTrackerDbContext _dbContext;
        public GetCurrentUserHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<CurrentUser> Handle(GetCurrentUser request, CancellationToken cancellationToken)
        {
            return await _dbContext.Users
                .Where(user => user.UserId == request.UserId)
                .Select(user => new CurrentUser
                {
                    UserId = user.UserId,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    EmailAddress = user.EmailAddress,
                    Created = user.Created,
                    WaterSize = user.WaterSize,
                    WaterTarget = user.WaterTarget,
                    CurrentPlan = user.UserPlans.OrderByDescending(up => up.Start).Select(up => up.Plan).FirstOrDefault(),
                    Started = user.UserPlans.OrderBy(up => up.Start).First().Start,
                })
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}