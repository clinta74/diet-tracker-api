using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.CQRS;
using diet_tracker_api.CQRS.Days;
using diet_tracker_api.CQRS.Users;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using diet_tracker_api.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
            var userId = _httpContextAccessor.HttpContext.User.Identity.Name;

            if (!await _mediator.Send(new UserExists(userId)))
            {
                return new NotFoundObjectResult($"User not found.");
            }

            var data = await _mediator.Send(new GetDay(day, userId), cancellationToken);

            return data;
        }

        [HttpPut("{day}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CurrentUserDay>> UpdateCurrentUserDay(DateTime day, UserDay userDay, CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext.User.Identity.Name;

            if (!await _mediator.Send(new UserExists(userId)))
            {
                return new NotFoundObjectResult($"User not found.");
            }

            await _mediator.Send(new UpdateDay(day, userId, userDay), cancellationToken);

            return await _mediator.Send(new GetDay(day, userId), cancellationToken);
        }
    }
}