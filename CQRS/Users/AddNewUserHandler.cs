using System;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using MediatR;

namespace diet_tracker_api.CQRS.Users
{
    public record AddNewUser(string UserId, string FirstName, string LastName, string EmailAddress, int PlanId) : IRequest<string>;
    public class AddNewUserHandler : IRequestHandler<AddNewUser, string>
    {
        private readonly DietTrackerDbContext _dbContext;
        public AddNewUserHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<string> Handle(AddNewUser request, CancellationToken cancellationToken)
        {

            var userPlans = new UserPlan[] { new UserPlan { PlanId = request.PlanId, Start = DateTime.Now }};

            var result = _dbContext.Users.Add(new User
            {
                UserId = request.UserId,
                FirstName = request.FirstName,
                LastName = request.LastName,
                EmailAddress = request.EmailAddress,
                Created = DateTime.Now,
                UserPlans = userPlans,
            });

            await _dbContext.SaveChangesAsync(cancellationToken);

            return result.Entity.UserId;
        }
    }
}