using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.CQRS.Users;
using diet_tracker_api.DataLayer.Models;
using diet_tracker_api.Extensions;
using MediatR;
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
    public class UserController
    {
        private readonly ILogger<UserController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMediator _mediator;

        public UserController(ILogger<UserController> logger, IHttpContextAccessor httpContextAccessor, IMediator mediator)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _mediator = mediator;
        }

        [Authorize("read:user")]
        [HttpGet("{userid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<User>> GetUserById(string userId, CancellationToken cancellationToken)
        {
            var data = await _mediator.Send(new GetUserById(userId));
            if (data == null) return new NotFoundResult();

            return data;
        }

        /**
        * Get the current user
        */
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CurrentUser>> GetCurrentUser(CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext.GetUserId();
            var data = await _mediator.Send(new GetCurrentUser(userId));

            if (data == null) return new NotFoundResult();

            return data;
        }

        [HttpGet("exists")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<bool>> GetCurrentUserExists(CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext.GetUserId();

            return await _mediator.Send(new UserExists(userId));
        }

    }
}