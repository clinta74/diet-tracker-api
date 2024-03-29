using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.BusinessLayer.Fuelings;
using diet_tracker_api.DataLayer.Models;
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

    public class FuelingController
    {
        private readonly ILogger<FuelingController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMediator _mediator;

        public FuelingController(ILogger<FuelingController> logger, IHttpContextAccessor httpContextAccessor, IMediator mediator)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _mediator = mediator;
        }

        [HttpGet("/api/fuelings")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult Get(CancellationToken cancellationToken)
        {
            return new OkObjectResult(_mediator.CreateStream(new GetFuelings(), cancellationToken));
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

            return await _mediator.Send(new AddFueling(fueling.Name), cancellationToken);
        }

        [Authorize("write:fuelings")]
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Update(int id, Fueling fueling, CancellationToken cancellationToken)
        {
            if (fueling == null)
            {
                return new BadRequestResult();
            }

            try
            {
                await _mediator.Send(new UpdateFueling(id, fueling.Name), cancellationToken);
                return new OkResult();
            }
            catch (ArgumentException ex)
            {
                return new NotFoundObjectResult(ex.Message);
            }
        }

        [Authorize("write:fuelings")]
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> Remove(int id, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _mediator.Send(new DeleteFueling(id), cancellationToken);
                return new OkResult();
            }
            catch (ArgumentException ex)
            {
                return new NotFoundObjectResult(ex.Message);
            }
        }
    }
}