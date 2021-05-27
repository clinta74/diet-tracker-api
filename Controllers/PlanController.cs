using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.CQRS;
using diet_tracker_api.CQRS.Plans;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using diet_tracker_api.Extensions;
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
        private readonly DietTrackerDbContext _dbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMediator _mediator;

        public PlanController(ILogger<PlanController> logger, DietTrackerDbContext dbContext, IHttpContextAccessor httpContextAccessor, IMediator mediator)
        {
            _logger = logger;
            _dbContext = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _mediator = mediator;
        }

        [HttpGet("/api/plans")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IAsyncEnumerable<Plan> Get(CancellationToken cancellationToken)
        {
            var data = _dbContext.Plans
                .AsNoTracking()
                .AsAsyncEnumerable();

            return data;
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Plan>> GetById(int id, CancellationToken cancellationToken)
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

            var result = _dbContext.Plans
                .Add(plan);

            await _dbContext.SaveChangesAsync(cancellationToken);

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

            var results = await _dbContext.Plans.FindAsync(id);

            if (results != null)
            {
                var data = results with
                {
                    Name = plan.Name,
                    FuelingCount = plan.FuelingCount,
                    MealCount = plan.MealCount,

                };
                _dbContext.Plans.Update(data);

                await _dbContext.SaveChangesAsync(cancellationToken);
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
            var plan = await _dbContext
                .Plans
                .FindAsync(id);

            if (plan != null)
            {
                _dbContext.Remove(plan);

                await _dbContext.SaveChangesAsync();
                return new OkResult();
            }

            return new NotFoundResult();
        }

        [HttpPut("change")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<int>> Change([FromBody]int planId, CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext.GetUserId();

            var user = await _dbContext.Users
                .FindAsync(userId);

            if (user == null)
            {
                return new NotFoundObjectResult($"User not found.");
            }

            return await _mediator.Send(new ChangeUserPlan(userId, planId));
        }
    }
}