using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
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
        private readonly DietTrackerDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public PlanController(ILogger<PlanController> logger, DietTrackerDbContext dietTrackerDbContext, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _context = dietTrackerDbContext;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet("/api/plans")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IAsyncEnumerable<Plan> Get(CancellationToken cancellationToken)
        {
            var data = _context.Plans
                .AsNoTracking()
                .AsAsyncEnumerable();

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

            var result = _context.Plans
                .Add(plan);

            await _context.SaveChangesAsync(cancellationToken);

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

            var results = await _context.Plans.FindAsync(id);

            if (results != null)
            {
                var data = results with
                {
                    Name = plan.Name,
                    FuelingCount = plan.FuelingCount,
                    MealCount = plan.MealCount,

                };
                _context.Plans.Update(data);

                await _context.SaveChangesAsync(cancellationToken);
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
            var plan = await _context
                .Plans
                .FindAsync(id, cancellationToken);

            if (plan != null)
            {
                _context.Remove(plan);

                await _context.SaveChangesAsync();
                return new OkResult();
            }

            return new NotFoundResult();
        }
    }
}