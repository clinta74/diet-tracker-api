using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.BusinessLayer.Users;
using diet_tracker_api.DataLayer.Models;
using diet_tracker_api.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMediator _mediator;

        public NewUserController(IHttpContextAccessor httpContextAccessor, IMediator mediator)
        {
            _httpContextAccessor = httpContextAccessor;
            _mediator = mediator;
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
        public ActionResult<NewUser> GetNewUser()
        {
            var userId = _httpContextAccessor.HttpContext.GetUserId();
            return new NewUser { UserId = userId };
        }
    }
}