using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.BusinessLayer.Plans;
using diet_tracker_api.BusinessLayer.Users;
using diet_tracker_api.DataLayer;
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
    public class PlanController
    {
        private readonly ILogger<PlanController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMediator _mediator;

        public PlanController(ILogger<PlanController> logger, IHttpContextAccessor httpContextAccessor, IMediator mediator)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _mediator = mediator;
        }

        [HttpGet("/api/plans")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IAsyncEnumerable<Plan> Get(CancellationToken cancellationToken)
        {
            return _mediator.Send(new GetPlans(), cancellationToken).Result;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Plan>> GetById([FromRoute] int id, CancellationToken cancellationToken)
        {
            var data = await _mediator.Send(new GetPlanById(id));
            if (data == null)
            {
                return new NotFoundResult();
            }

            return data;
        }

        [Authorize("write:plans")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<int>> Add(Plan plan, CancellationToken cancellationToken)
        {
            if (plan == null)
            {
                return new BadRequestResult();
            }

            return await _mediator.Send(new AddPlan(plan.Name, plan.FuelingCount, plan.MealCount));
        }

        [Authorize("write:plans")]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update([FromRoute] int id, [FromBody] Plan plan, CancellationToken cancellationToken)
        {
            if (plan == null)
            {
                return new BadRequestResult();
            }

            try
            {
                await _mediator.Send(new UpdatePlan(id, plan.Name, plan.FuelingCount, plan.MealCount), cancellationToken);
                return new OkResult();
            }
            catch (ArgumentException ex)
            {
                return new NotFoundObjectResult(ex.Message);
            }
        }

        [Authorize("write:plans")]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Remove([FromRoute] int id, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _mediator.Send(new DeletePlan(id), cancellationToken);
                return new OkResult();
            }
            catch (ArgumentException ex)
            {
                return new NotFoundObjectResult(ex.Message);
            }
        }

        [HttpPut("change")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<int>> Change([FromBody] int planId, CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext.GetUserId();

            var userExists = await _mediator.Send(new UserExists(userId));

            if (userExists)
            {
                return await _mediator.Send(new ChangeUserPlan(userId, planId));
            }

            return new NotFoundObjectResult($"User not found.");
        }
    }
}