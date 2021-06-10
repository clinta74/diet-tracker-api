using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.CQRS.UserTrackings;
using diet_tracker_api.DataLayer.Models;
using diet_tracker_api.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace diet_tracker_api.Controllers
{
    public record UserTrackingRequest(string Title, string Description, int Occurrences, int Order, bool Disabled, IEnumerable<UserTrackingValueRequest> Values);
    public record UserTrackingValueRequest(int UserTrackingId, string Name, string Description, UserTrackingType Type, int Order, bool Disabled);

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
            var userId = _httpContextAccessor.HttpContext.GetUserId();
            return await _mediator.Send(new GetActiveUserTrackings(userId));
        }

        [HttpGet("/api/user-trackings")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IEnumerable<UserTracking>> GetAll(CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext.GetUserId();
            return await _mediator.Send(new GetUserTrackings(userId));
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserTracking>> GetById(int id, CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext.GetUserId();
            var data = await _mediator.Send(new GetUserTracking(userId, id));

            if (data == null)
            {
                return new NotFoundResult();
            }

            return data;
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<int>> Add(UserTrackingRequest userTracking)
        {
            if (userTracking == null) return new BadRequestResult();

            var userId = _httpContextAccessor.HttpContext.GetUserId();
            return await _mediator.Send(new AddUserTracking
            (
                userId,
                userTracking.Title,
                userTracking.Description,
                userTracking.Occurrences,
                userTracking.Order,
                userTracking.Values.Select(value => new UserTrackingValue
                {
                    Name = value.Name,
                    Description = value.Description,
                    Order = value.Order,
                    Type = value.Type,
                    Disabled = value.Disabled,
                })
            ));
        }

        [HttpPut("{userTrackingId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Update(int userTrackingId, UserTrackingRequest userTracking)
        {
            if (userTracking == null) return new BadRequestResult();

            try
            {
                var userId = _httpContextAccessor.HttpContext.GetUserId();
                var data = await _mediator.Send(new UpdateUserTracking
                (
                    userId,
                    userTrackingId,
                    userTracking.Title,
                    userTracking.Description,
                    userTracking.Occurrences,
                    userTracking.Disabled,
                    userTracking.Values.Select(value => new UserTrackingValue
                    {
                        Name = value.Name,
                        Description = value.Description,
                        Order = value.Order,
                        Type = value.Type,
                        Disabled = value.Disabled,
                    })
                ));

                return new OkResult();
            }
            catch (ArgumentException ex)
            {
                return new NotFoundObjectResult(ex.Message);
            }
        }

        [HttpDelete("{userTrackingId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Delete(int userTrackingId)
        {
            var userId = _httpContextAccessor.HttpContext.GetUserId();
            var data = await _mediator.Send(new DeleteUserTracking(userTrackingId, userId));

            if (data == false) return new NotFoundResult();

            return new OkResult();
        }
    }
}