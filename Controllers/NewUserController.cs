using System;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using diet_tracker_api.Models;
using diet_tracker_api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace diet_tracker_api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class NewUserController
    {
        private readonly IAuth0ManagementApiClient _managementApiClient;
        private readonly ILogger<NewUserController> _logger;
        private readonly DietTrackerDbContext _dietTrackerDbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        
        public NewUserController(ILogger<NewUserController> logger, IAuth0ManagementApiClient managementApiClient, DietTrackerDbContext dietTrackerDbContext, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _managementApiClient = managementApiClient;
            _dietTrackerDbContext = dietTrackerDbContext;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost("create")]
        public async Task<User> CreateNewUser(CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext.User.Identity.Name;
            var userData = await _managementApiClient.Client.Users.GetAsync(userId);

            var result = _dietTrackerDbContext.Users.Add(new User
            {
                UserId = userId,
                FirstName = userData.FirstName,
                LastName = userData.LastName,
                EmailAddress = userData.Email,
                Created = DateTime.Now
            });

            await _dietTrackerDbContext.SaveChangesAsync(cancellationToken);

            return result.Entity;
        }

        [HttpPost]
        public async Task<ActionResult<string>> AddNewUser(NewUser userData, CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext.User.Identity.Name;

            var userPlans = new UserPlan[] { new UserPlan { PlanId = userData.PlanId, Start = DateTime.Now }};

            var result = _dietTrackerDbContext.Users.Add(new User
            {
                UserId = userId,
                FirstName = userData.FirstName,
                LastName = userData.LastName,
                EmailAddress = userData.EmailAddress,
                Created = DateTime.Now,
                UserPlans = userPlans,
            });

            await _dietTrackerDbContext.SaveChangesAsync(cancellationToken);

            return result.Entity.UserId;
        }

        [HttpGet]
        public async Task<ActionResult<NewUser>> GetNewUser(CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext.User.Identity.Name;
            var userData = await _managementApiClient.Client.Users.GetAsync(userId);

            return new NewUser
            {
                UserId = userId,
                FirstName = userData.FirstName,
                LastName = userData.LastName,
                EmailAddress = userData.Email,
                LastLogin = DateTime.Now
            };
        }
    }
}