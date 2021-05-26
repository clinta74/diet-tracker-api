using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.CQRS.UserTrackings;
using diet_tracker_api.DataLayer.Models;
using diet_tracker_api.Models;
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
    public class UserTrackingController
    {
        private readonly ILogger<UserTrackingController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMediator _mediator;

        public UserTrackingController(ILogger<UserTrackingController> logger, IHttpContextAccessor httpContextAccessor, IMediator mediator)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _mediator = mediator;
        }

        [HttpGet("/api/user-trackings/active")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IEnumerable<UserTracking>> GetAllActive(CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext.User.Identity.Name;
            return await _mediator.Send(new GetActiveUserTrackings(userId));
        }

        [HttpGet("/api/user-trackings")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IEnumerable<UserTracking>> GetAll(CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext.User.Identity.Name;
            return await _mediator.Send(new GetUserTrackings(userId));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<int> Add(UserTrackingRequest userTracking)
        {
            var userId = _httpContextAccessor.HttpContext.User.Identity.Name;            
            return await _mediator.Send(new AddUserTracking
            (
                userId, 
                userTracking.Title, 
                userTracking.Description, 
                userTracking.Occurrences,
                userTracking.Order
            ));
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Update(int id, UserTrackingRequest userTracking)
        {
            if (userTracking == null) return new BadRequestResult();
            var userId = _httpContextAccessor.HttpContext.User.Identity.Name;
            var data = await _mediator.Send(new UpdateUserTracking
            (
                id, 
                userTracking.Title, 
                userTracking.Description, 
                userTracking.Occurrences,
                userTracking.Disabled
            ));

            if (data == false) return new NotFoundResult();

            return new OkResult();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Delete(int id)
        {
            var data = await _mediator.Send(new DeleteUserTracking(id));

            if (data == false) return new NotFoundResult();

            return new OkResult();
        }
    }
}