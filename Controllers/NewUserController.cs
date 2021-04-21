using System;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
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
        private readonly ILogger _logger;
        private readonly DietTrackerDbContext _dietTrackerDbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        
        public NewUserController(ILogger<NewUserController> logger, IAuth0ManagementApiClient managementApiClient, DietTrackerDbContext dietTrackerDbContext, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _managementApiClient = managementApiClient;
            _dietTrackerDbContext = dietTrackerDbContext;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost]
        public async Task<User> AddNewUser()
        {
            var userId = _httpContextAccessor.HttpContext.User.Identity.Name;
            var userData = await _managementApiClient.Client.Users.GetAsync(userId);

            var result = _dietTrackerDbContext.Users.Add(new User
            {
                UserId = userId,
                FristName = userData.FirstName,
                LastName = userData.LastName,
                EmailAddress = userData.Email,
                LastLogin = DateTime.Now
            });

            await _dietTrackerDbContext.SaveChangesAsync();

            return result.Entity;
        }
    }
}