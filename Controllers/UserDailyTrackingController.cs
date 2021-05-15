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
                var remaining = activeUserTracking.Occurances - currentUserDailyTrackings.Count(c => c.UserTrackingId == activeUserTracking.UserTrackingId);
                var newCurrentUserDailyTrackings = new List<CurrentUserDailyTracking>();

                for (int idx = 0; idx < remaining; idx++)
                {
                    newCurrentUserDailyTrackings.Add(new CurrentUserDailyTracking
                    {
                        Value = 0,
                        Day = day,
                        UserTrackingId = activeUserTracking.UserTrackingId,
                        UserId = userId,
                        Name = activeUserTracking.Name,
                        Description = activeUserTracking.Description,
                        Occurance = idx + remaining + 1,
                    });
                }

                currentUserDailyTrackings.AddRange(newCurrentUserDailyTrackings);
            }

            return new OkObjectResult(currentUserDailyTrackings);
        }

        [HttpPut]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<UserDailyTracking> UpdateCurrentUserDailyTracking(UserDailyTracking userDailyTracking)
        {
            var userId = _httpContextAccessor.HttpContext.User.Identity.Name;

            return await _mediator.Send(new UpsertUserDailyTracking
                (
                    userDailyTracking.Day,
                    userDailyTracking.UserId,
                    userDailyTracking.UserTrackingId,
                    userDailyTracking.Occurance,
                    userDailyTracking.Value,
                    userDailyTracking.When
                )
            );
        }
    }
}