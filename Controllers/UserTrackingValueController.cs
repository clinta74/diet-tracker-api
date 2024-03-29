using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.BusinessLayer.UserTrackingValues;
using diet_tracker_api.Controllers.Models;
using diet_tracker_api.DataLayer.Models;
using diet_tracker_api.Extensions;
using diet_tracker_api.Filters;
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
    [ServiceFilter(typeof(UserExistsFilter))]
    public class UserTrackingValueController
    {
        private readonly ILogger<UserTrackingValueController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMediator _mediator;

        public UserTrackingValueController(ILogger<UserTrackingValueController> logger, IHttpContextAccessor httpContextAccessor, IMediator mediator)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _mediator = mediator;
        }

        [HttpGet("/api/user-tracking-values/user-tracking/{userTrackingId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IEnumerable<UserTrackingValue>> GetAll(int userTrackingId, CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext.GetUserId();
            return await _mediator.Send(new GetUserTrackingValues(userId, userTrackingId));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<int>> Add(UserTrackingValueRequest userTrackingValue)
        {
            if (userTrackingValue == null) return new BadRequestResult();

            var userId = _httpContextAccessor.HttpContext.GetUserId();
            return await _mediator.Send(new AddUserTrackingValue
            (
                userTrackingValue.UserTrackingId,
                userTrackingValue.Name,
                userTrackingValue.Description,
                userTrackingValue.Order,
                userTrackingValue.Type,
                userTrackingValue.Disabled,
                userTrackingValue.Metadata
            ));
        }

        [HttpPut("{userTrackingValueId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Update(int userTrackingValueId, UserTrackingValueRequest userTrackingValue)
        {
            if (userTrackingValue == null) return new BadRequestResult();

            var userId = _httpContextAccessor.HttpContext.GetUserId();
            var data = await _mediator.Send(new UpdateUserTrackingValue
            (
                userTrackingValueId,
                userId,
                userTrackingValue.Name,
                userTrackingValue.Description,
                userTrackingValue.Order,
                userTrackingValue.Type,
                userTrackingValue.Disabled,
                userTrackingValue.Metadata
            ));

            if (data == false) return new NotFoundResult();

            return new OkResult();
        }

        [HttpDelete("{userTrackingValueId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Delete(int userTrackingValueId)
        {
            try
            {
                var userId = _httpContextAccessor.HttpContext.GetUserId();
                var data = await _mediator.Send(new DeleteUserTrackingValue(userId, userTrackingValueId));
                return new OkResult();
            }
            catch (ArgumentException ex)
            {
                return new NotFoundObjectResult(ex.Message);
            }
        }
    }
}