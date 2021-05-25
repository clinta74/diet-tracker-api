using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.CQRS.UserDailyTrackings;
using diet_tracker_api.CQRS.Users;
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
    public class UserDailyTrackingController
    {
        private readonly ILogger<UserDailyTrackingController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMediator _mediator;

        public UserDailyTrackingController(ILogger<UserDailyTrackingController> logger, IHttpContextAccessor httpContextAccessor, IMediator mediator)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _mediator = mediator;
        }

        [HttpGet("{day}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<CurrentUserDailyTracking>>> GetCurrent(DateTime day, CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext.User.Identity.Name;

            if (!await _mediator.Send(new UserExists(userId)))
            {
                return new NotFoundObjectResult($"User not found.");
            }

            var activeUserTrackings = await _mediator.Send(new GetActiveUserTrackings(userId));
            var currentUserDailyTrackings = _mediator.Send(new GetCurrentUserDailyTrackings(day, userId)).Result.ToList();

            foreach (var activeUserTracking in activeUserTrackings)
            {
                var remaining = activeUserTracking.Occurrences - currentUserDailyTrackings.Count(c => c.UserTrackingId == activeUserTracking.UserTrackingId);
                var newCurrentUserDailyTrackings = new List<CurrentUserDailyTracking>();

                for (int idx = 0; idx < remaining; idx++)
                {
                    newCurrentUserDailyTrackings.Add(new CurrentUserDailyTracking
                    {
                        Values = activeUserTracking.Values.Select(v => new CurrentUserDailyTrackingValue
                        {
                            Value = 0,
                            Name = v.Name,
                            Order = v.Order,
                        }),
                        Day = day,
                        UserTrackingId = activeUserTracking.UserTrackingId,
                        UserId = userId,
                        Title = activeUserTracking.Title,
                        Description = activeUserTracking.Description,
                        Occurrence = idx + remaining,
                    });
                }

                currentUserDailyTrackings.AddRange(newCurrentUserDailyTrackings);
            }

            return new OkObjectResult(currentUserDailyTrackings);
        }

        [HttpPut("{day}/{userTrackingId}/{occurrence}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<UserDailyTracking> UpdateCurrentUserDailyTracking(DateTime day, int userTrackingId, int occurrence, IEnumerable<CurrentUserDailyTrackingValueRequest> values, CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext.User.Identity.Name;

            return await _mediator.Send(new UpsertUserDailyTracking
                (
                    day,
                    userId,
                    userTrackingId,
                    occurrence,
                    values
                )
            , cancellationToken);
        }
    }
}