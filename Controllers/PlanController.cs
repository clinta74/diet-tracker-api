using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.CQRS;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
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
    public class PlanController
    {
        private readonly ILogger<PlanController> _logger;
        private readonly DietTrackerDbContext _dietTrackerDbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMediator _mediator;

        public PlanController(ILogger<PlanController> logger, DietTrackerDbContext dietTrackerDbContext, IHttpContextAccessor httpContextAccessor, IMediator mediator)
        {
            _logger = logger;
            _dietTrackerDbContext = dietTrackerDbContext;
            _httpContextAccessor = httpContextAccessor;
            _mediator = mediator;
        }

        [HttpGet("/api/plans")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IAsyncEnumerable<Plan> Get(CancellationToken cancellationToken)
        {
            var data = _dietTrackerDbContext.Plans
                .AsNoTracking()
                .AsAsyncEnumerable();

            return data;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Plan>> GetById(int id, CancellationToken cancellationToken)
        {
            var data = await _mediator.Send(new Plans.GetPlanById(id));
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

            var result = _dietTrackerDbContext.Plans
                .Add(plan);

            await _dietTrackerDbContext.SaveChangesAsync(cancellationToken);

            return result.Entity.PlanId;
        }

        [Authorize("write:plans")]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(int id, Plan plan, CancellationToken cancellationToken)
        {
            if (plan == null)
            {
                return new BadRequestResult();
            }

            var results = await _dietTrackerDbContext.Plans.FindAsync(id);

            if (results != null)
            {
                var data = results with
                {
                    Name = plan.Name,
                    FuelingCount = plan.FuelingCount,
                    MealCount = plan.MealCount,

                };
                _dietTrackerDbContext.Plans.Update(data);

                await _dietTrackerDbContext.SaveChangesAsync(cancellationToken);
                return new OkResult();
            }

            return new NotFoundResult();
        }

        [Authorize("write:plans")]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Remove(int id, CancellationToken cancellationToken)
        {
            var plan = await _dietTrackerDbContext
                .Plans
                .FindAsync(id, cancellationToken);

            if (plan != null)
            {
                _dietTrackerDbContext.Remove(plan);

                await _dietTrackerDbContext.SaveChangesAsync();
                return new OkResult();
            }

            return new NotFoundResult();
        }

        [HttpPut("change")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<int>> Change([FromBody]int planId, CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext.User.Identity.Name;

            var user = await _dietTrackerDbContext.Users
                .FindAsync(userId);

            if (user == null)
            {
                return new NotFoundObjectResult($"User not found.");
            }

            return await _mediator.Send(new Plans.ChangeUserPlan(userId, planId));
        }
    }
}