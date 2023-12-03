using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.BusinessLayer.Plans;
using diet_tracker_api.BusinessLayer.Users;
using diet_tracker_api.DataLayer.Models;
using diet_tracker_api.Extensions;
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
        public IActionResult Get(CancellationToken cancellationToken)
        {
            return new OkObjectResult(_mediator.Send(new GetPlans(), cancellationToken));
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Plan>> GetById(int id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetPlanById(id), cancellationToken);

            return result.Match<ActionResult>(r => new OkObjectResult(r as Plan), r => new NotFoundObjectResult(r.Message));
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
        public async Task<ActionResult<Plan>> Update(int id, Plan plan, CancellationToken cancellationToken)
        {
            if (plan == null)
            {
                return new BadRequestResult();
            }

            var result = await _mediator.Send(new UpdatePlan(id, plan.Name, plan.FuelingCount, plan.MealCount), cancellationToken);

            return result.Match<ActionResult>(r => new OkObjectResult(r as Plan), r => new NotFoundObjectResult(r.Message));
        }

        [Authorize("write:plans")]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Remove(int id, CancellationToken cancellationToken)
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
                return await _mediator.Send(new ChangeUserPlan(userId, planId), cancellationToken);
            }

            return new NotFoundObjectResult($"User not found.");
        }
    }
}