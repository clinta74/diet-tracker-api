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
        private readonly DietTrackerDbContext _dietTrackerDbContext;
        public UserExistsHandler(DietTrackerDbContext dietTrackerDbContext)
        {
            _dietTrackerDbContext = dietTrackerDbContext;
        }

        public async Task<bool> Handle(UserExists request, CancellationToken cancellationToken)
        {
            var user = await _dietTrackerDbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(user => user.UserId == request.UserId);

            return user != null;
        }
    }

    public class GetUserByIdHandler : IRequestHandler<Users.GetUserById, User>
    {
        private readonly DietTrackerDbContext _dietTrackerDbContext;
        public GetUserByIdHandler(DietTrackerDbContext dietTrackerDbContext)
        {
            _dietTrackerDbContext = dietTrackerDbContext;
        }

        public async Task<User> Handle(GetUserById request, CancellationToken cancellationToken)
        {
            return await _dietTrackerDbContext.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(user => user.UserId == request.UserId);
        }
    }

    public class GetCurrentUserHandler : IRequestHandler<Users.GetCurrentUser, CurrentUser>
    {
        private readonly DietTrackerDbContext _dietTrackerDbContext;
        public GetCurrentUserHandler(DietTrackerDbContext dietTrackerDbContext)
        {
            _dietTrackerDbContext = dietTrackerDbContext;
        }

        public async Task<CurrentUser> Handle(GetCurrentUser request, CancellationToken cancellationToken)
        {
            return await _dietTrackerDbContext.Users
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