using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.BusinessLayer.Users;
using diet_tracker_api.DataLayer.Models;
using diet_tracker_api.Extensions;
using diet_tracker_api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.Extensions.Logging;

namespace diet_tracker_api.Controllers
{
    public record NewUser
    {
        public string UserId { get; init; }
        public string FirstName { get; init; }
        public string LastName { get; init; }
        public string EmailAddress { get; init; }
        public int PlanId { get; init; }
    }
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class NewUserController
    {
        private readonly IAuth0ManagementApiClient _managementApiClient;
        private readonly ILogger<NewUserController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMediator _mediator;

        public NewUserController(ILogger<NewUserController> logger, IAuth0ManagementApiClient managementApiClient, IHttpContextAccessor httpContextAccessor, IMediator mediator)
        {
            _logger = logger;
            _managementApiClient = managementApiClient;
            _httpContextAccessor = httpContextAccessor;
            _mediator = mediator;
        }

        [HttpPost("create")]
        public async Task<User> CreateNewUser(CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext.GetUserId();
            var userData = await _managementApiClient.Client.Users.GetAsync(userId);

            var data = await _mediator.Send(
                new CreateNewUser(userId, userData.FirstName, userData.LastName, userData.Email),
                cancellationToken
            );

            return data;
        }

        [HttpPost]
        public async Task<ActionResult<string>> AddNewUser(NewUser userData, CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext.GetUserId();

            return await _mediator.Send(
                new AddNewUser(userId, userData.FirstName, userData.LastName, userData.EmailAddress, userData.PlanId),
                cancellationToken
            );
        }

        [HttpGet]
        public async Task<ActionResult<NewUser>> GetNewUser(CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext.GetUserId();
            var userData = await _managementApiClient.Client.Users.GetAsync(userId, null, true, cancellationToken);

            return new NewUser
            {
                UserId = userId,
                FirstName = userData.FirstName,
                LastName = userData.LastName,
                EmailAddress = userData.Email
            };
        }
    }
}