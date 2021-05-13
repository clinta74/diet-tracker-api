using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.CQRS.Fuelings;
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

    public class FuelingController
    {
        private readonly ILogger<FuelingController> _logger;
        private readonly DietTrackerDbContext _context;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMediator _mediator;

        public FuelingController(ILogger<FuelingController> logger, DietTrackerDbContext dbContext, IHttpContextAccessor httpContextAccessor, IMediator mediator)
        {
            _logger = logger;
            _context = dbContext;
            _httpContextAccessor = httpContextAccessor;
            _mediator = mediator;
        }

        [HttpGet("/api/fuelings")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IAsyncEnumerable<Fueling> Get()
        {
            return _mediator.Send(new GetFuelings()).Result;
        }

        [Authorize("write:fuelings")]
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Fueling>> Add(Fueling fueling, CancellationToken cancellationToken)
        {
            if (fueling == null)
            {
                return new BadRequestResult();
            }

            var result = _context.Fuelings
                .Add(fueling);

            await _context.SaveChangesAsync(cancellationToken);

            return result.Entity;
        }

        [Authorize("write:fuelings")]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<int>> Update(int id, Fueling fueling, CancellationToken cancellationToken)
        {
            if (fueling == null)
            {
                return new BadRequestResult();
            }

            var results = await _context.Fuelings
                .AsNoTracking()
                .FirstOrDefaultAsync(fueling => fueling.FuelingId == id);

            if (results != null)
            {
                var data = results with
                {
                    Name = fueling.Name,
                };
                _context.Fuelings.Update(data);

                await _context.SaveChangesAsync(cancellationToken);
                return new OkResult();
            }

            return new NotFoundResult();
        }

        [Authorize("write:fuelings")]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Remove(int id, CancellationToken cancellationToken)
        {
            var fueling = await _context
                .Fuelings
                .FindAsync(id);

            if (fueling != null)
            {
                _context.Remove(fueling);

                await _context.SaveChangesAsync(cancellationToken);
                return new OkResult();
            }

            return new NotFoundResult();
        }
    }
}