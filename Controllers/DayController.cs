using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.BusinessLayer.Days;
using diet_tracker_api.BusinessLayer.Days.Fuelings;
using diet_tracker_api.BusinessLayer.Days.Meals;
using diet_tracker_api.BusinessLayer.Days.Victories;
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
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class DayController
    {
        private readonly ILogger<DayController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMediator _mediator;

        public DayController(ILogger<DayController> logger, IHttpContextAccessor httpContextAccessor, IMediator mediator)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _mediator = mediator;
        }

        [HttpGet("{day}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CurrentUserDay>> GetCurrentUserDay(DateTime day, CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext.GetUserId();

            if (!await _mediator.Send(new UserExists(userId)))
            {
                return new NotFoundObjectResult($"User not found.");
            }

            return new OkObjectResult(await _mediator.Send(new GetDay(day, userId), cancellationToken));
        }

        [HttpGet("{day}/fuelings")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<UserDayFueling>>> GetCurrentUserDayFuelings(DateTime day, CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext.GetUserId();

            if (!await _mediator.Send(new UserExists(userId)))
            {
                return new NotFoundObjectResult($"User not found.");
            }

            return new OkObjectResult(await _mediator.Send(new GetDayFuelings(day, userId), cancellationToken));
        }

        [HttpGet("{day}/meals")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<UserDayMeal>>> GetCurrentUserDayMeals(DateTime day, CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext.GetUserId();

            if (!await _mediator.Send(new UserExists(userId)))
            {
                return new NotFoundObjectResult($"User not found.");
            }

            return new OkObjectResult(await _mediator.Send(new GetDayMeals(day, userId), cancellationToken));
        }

        [HttpGet("{day}/victories")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<UserDayVictory>>> GetCurrentUserDayVictories(DateTime day, CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext.GetUserId();

            if (!await _mediator.Send(new UserExists(userId)))
            {
                return new NotFoundObjectResult($"User not found.");
            }

            return new OkObjectResult(await _mediator.Send(new GetDayVictories(day, userId), cancellationToken));
        }

        [HttpPut("{day}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CurrentUserDay>> UpdateCurrentUserDay(DateTime day, CurrentUserDay userDay, CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext.GetUserId();

            if (!await _mediator.Send(new UserExists(userId)))
            {
                return new NotFoundObjectResult($"User not found.");
            }

            await _mediator.Send(new UpdateDay(day, userId, userDay), cancellationToken);

            return await _mediator.Send(new GetDay(day, userId), cancellationToken);
        }

        [HttpPut("{day}/fuelings")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<UserDayFueling>>> UpdateDayFuelings(DateTime day, IEnumerable<UserFueling> fuelings, CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext.GetUserId();

            if (!await _mediator.Send(new UserExists(userId)))
            {
                return new NotFoundObjectResult($"User not found.");
            }

            await _mediator.Send(new UpdateDayFuelings(day, userId, fuelings), cancellationToken);

            return new OkObjectResult(await _mediator.Send(new GetDayFuelings(day, userId), cancellationToken));
        }

        [HttpPut("{day}/meals")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<UserDayMeal>>> UpdateDayMeals(DateTime day, IEnumerable<UserMeal> meals, CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext.GetUserId();

            if (!await _mediator.Send(new UserExists(userId)))
            {
                return new NotFoundObjectResult($"User not found.");
            }

            await _mediator.Send(new UpdateDayMeals(day, userId, meals), cancellationToken);

            return new OkObjectResult(await _mediator.Send(new GetDayMeals(day, userId), cancellationToken));
        }

        [HttpPut("{day}/victories")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<UserDayVictory>>> UpdateDayVictories(DateTime day, IEnumerable<Victory> victories, CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext.GetUserId();

            if (!await _mediator.Send(new UserExists(userId)))
            {
                return new NotFoundObjectResult($"User not found.");
            }

            await _mediator.Send(new UpdateDayVictories(day, userId, victories), cancellationToken);

            return new OkObjectResult(await _mediator.Send(new GetDayVictories(day, userId), cancellationToken));
        }

        [HttpGet("weight")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IAsyncEnumerable<GraphValue>>> GetWeight(DateTime startDate, Nullable<DateTime> endDate = null)
        {
            var userId = _httpContextAccessor.HttpContext.GetUserId();

            if (!await _mediator.Send(new UserExists(userId)))
            {
                return new NotFoundObjectResult($"User not found.");
            }

            return new OkObjectResult(await _mediator.Send(new GetWeightGraphData(userId, startDate, endDate)));
        }

        [HttpGet("water")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IAsyncEnumerable<GraphValue>>> GetWater(DateTime startDate, Nullable<DateTime> endDate = null)
        {
            var userId = _httpContextAccessor.HttpContext.GetUserId();

            if (!await _mediator.Send(new UserExists(userId)))
            {
                return new NotFoundObjectResult($"User not found.");
            }

            return new OkObjectResult(await _mediator.Send(new GetWaterGraphData(userId, startDate, endDate)));
        }
    }
}