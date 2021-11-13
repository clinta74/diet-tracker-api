using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.BusinessLayer.UserDailyTrackingValues;
using diet_tracker_api.BusinessLayer.Users;
using diet_tracker_api.DataLayer.Models;
using diet_tracker_api.Extensions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace diet_tracker_api.Controllers
{

    public record UserDailyTrackingValueRequest(int Occurrence, int UserTrackingValueId, decimal Value, Nullable<DateTime> When);

    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class DayTrackingValuesController
    {
        private readonly ILogger<DayTrackingValuesController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMediator _mediator;

        public DayTrackingValuesController(ILogger<DayTrackingValuesController> logger, IHttpContextAccessor httpContextAccessor, IMediator mediator)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _mediator = mediator;
        }

        [HttpGet("{day}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<UserDailyTrackingValue>>> GetCurrentUserDayTrackingValues([FromRoute] DateTime day, CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext.GetUserId();

            if (!await _mediator.Send(new UserExists(userId)))
            {
                return new NotFoundObjectResult($"User not found.");
            }

            var data = await _mediator.Send(new GetCurrentUserDailyTrackingValues(day.ToDateOnly(), userId), cancellationToken);

            return new OkObjectResult(data);
        }

        [HttpPut("{day}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserDailyTrackingValue>> UpdateCurrentUserDayTrackingValue([FromRoute] DateTime day, UserDailyTrackingValueRequest[] values, CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext.GetUserId();

            if (!await _mediator.Send(new UserExists(userId)))
            {
                return new NotFoundObjectResult($"User not found.");
            }

            var data = await _mediator.Send(new UpdateUserDailyTrackingValues(day.ToDateOnly(), userId, values.Select(value => 
                new UpdateUserDailyTrackingValue(value.UserTrackingValueId, value.Occurrence, value.Value, value.When)).ToArray()));

            return new OkObjectResult(data);
        }

        [HttpGet("{userTrackingId}/history")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IAsyncEnumerable<UserDailyTrackingValue>>> GetHistory([FromRoute] int userTrackingId, [FromQuery] DateOnly startDate, [FromQuery] Nullable<DateOnly> endDate = null)
        {
            var userId = _httpContextAccessor.HttpContext.GetUserId();

            if (!await _mediator.Send(new UserExists(userId)))
            {
                return new NotFoundObjectResult($"User not found.");
            }

            return new OkObjectResult(await _mediator.Send(new GetCurrentUserDailyTrackingValuesHistory(userId, userTrackingId, startDate, endDate)));
        } 
    }
}