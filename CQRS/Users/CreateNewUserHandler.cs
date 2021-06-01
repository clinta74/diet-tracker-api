using System;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using MediatR;

namespace diet_tracker_api.CQRS.Users
{
    public record CreateNewUser(string UserId, string FirstName, string LastName, string EmailAddress) : IRequest<User>;
    public class CreateNewUserHandler : IRequestHandler<Users.CreateNewUser, User>
    {
        private readonly DietTrackerDbContext _dbContext;
        public CreateNewUserHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<User> Handle(CreateNewUser request, CancellationToken cancellationToken)
        {

            var result = _dbContext
                .Users
                .Add(new User
                {
                    UserId = request.UserId,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    EmailAddress = request.EmailAddress,
                    Created = DateTime.Now
                });

            await _dbContext.SaveChangesAsync(cancellationToken);

            return result.Entity;
        }
    }
}