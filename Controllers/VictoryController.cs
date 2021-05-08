using System.Collections.Generic;
using System.Threading.Tasks;
using diet_tracker_api.CQRS;
using diet_tracker_api.CQRS.Users;
using diet_tracker_api.CQRS.Victories;
using diet_tracker_api.DataLayer.Models;
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
    public class VictoryController
    {
        private readonly ILogger<VictoryController> _logger;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMediator _mediator;

        public VictoryController(ILogger<VictoryController> logger, IHttpContextAccessor httpContextAccessor, IMediator mediator)
        {
            _logger = logger;
            _httpContextAccessor = httpContextAccessor;
            _mediator = mediator;
        }

        [HttpGet("/api/victories")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IEnumerable<Victory>> GetAll([FromQuery]VictoryType type)
        {
            var userId = _httpContextAccessor.HttpContext.User.Identity.Name;
            return await _mediator.Send(new GetVictories(userId, type));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<Victory> Add(Victory victory)
        {
            var userId = _httpContextAccessor.HttpContext.User.Identity.Name;            
            return await _mediator.Send(new AddVictory(userId, victory.Name, victory.When, victory.Type));
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Victory>> Update(int id, Victory victory)
        {
            if (victory == null) return new BadRequestResult();
            var userId = _httpContextAccessor.HttpContext.User.Identity.Name;
            var data = await _mediator.Send(new UpdateVictory(id, userId, victory.Name, victory.When, victory.Type));

            if (data == false) return new NotFoundResult();

            return new OkResult();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Victory>> Delete(int id)
        {
            var data = await _mediator.Send(new DeleteVictory(id));

            if (data == false) return new NotFoundResult();

            return new OkResult();
        }
    }
}