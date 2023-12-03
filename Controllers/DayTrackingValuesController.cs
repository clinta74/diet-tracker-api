using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.BusinessLayer.UserDailyTrackingValues;
using diet_tracker_api.DataLayer.Models;
using diet_tracker_api.Extensions;
using diet_tracker_api.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace diet_tracker_api.Controllers
{
    [ServiceFilter(typeof(UserExistsFilter))]
    public record UserDailyTrackingValueRequest(int Occurrence, int UserTrackingValueId, decimal Value, DateTime? When);

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
        public async Task<ActionResult<IEnumerable<UserDailyTrackingValue>>> GetCurrentUserDayTrackingValues(DateTime day, CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext.GetUserId();
            var data = await _mediator.Send(new GetCurrentUserDailyTrackingValues(day, userId), cancellationToken);

            return new OkObjectResult(data);
        }

        [HttpPut("{day}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<UserDailyTrackingValue>> UpdateCurrentUserDayTrackingValue(DateTime day, UserDailyTrackingValueRequest[] values, CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext.GetUserId();
            var data = await _mediator.Send(new UpdateUserDailyTrackingValues(day, userId, values.Select(value =>
                new UpdateUserDailyTrackingValue(value.UserTrackingValueId, value.Occurrence, value.Value, value.When)).ToArray()));

            return new OkObjectResult(data);
        }

        [HttpGet("{userTrackingId}/history")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IAsyncEnumerable<UserDailyTrackingValue>>> GetHistory(int userTrackingId, DateTime startDate, DateTime? endDate = null)
        {
            var userId = _httpContextAccessor.HttpContext.GetUserId();
            return new OkObjectResult(await _mediator.Send(new GetCurrentUserDailyTrackingValuesHistory(userId, userTrackingId, startDate, endDate)));
        }
    }
}