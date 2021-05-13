using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.CQRS.Users;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using diet_tracker_api.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.CQRS
{
    namespace Users
    {
        public record UserExists(string UserId) : IRequest<bool>;
        public record GetUserById(string UserId) : IRequest<User>;
        public record GetCurrentUser(string UserId) : IRequest<CurrentUser>;
    }

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

    public class GetUserByIdHandler : IRequestHandler<Users.GetUserById, User>
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

    public class GetCurrentUserHandler : IRequestHandler<Users.GetCurrentUser, CurrentUser>
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
                    CurrentPlan = user.UserPlans.OrderByDescending(up => up.Start).Select(up => up.Plan).FirstOrDefault(),
                    Started = user.UserPlans.OrderBy(up => up.Start).First().Start,
                })
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);
        }
    }
}