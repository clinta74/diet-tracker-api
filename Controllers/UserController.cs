using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using diet_tracker_api.Models;
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
    public class UserController
    {
        private readonly ILogger<UserController> _logger;
        private readonly DietTrackerDbContext _dietTrackerDbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public UserController(ILogger<UserController> logger, DietTrackerDbContext dietTrackerDbContext, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _dietTrackerDbContext = dietTrackerDbContext;
            _httpContextAccessor = httpContextAccessor;
        }

        [Authorize("read:user")]
        [HttpGet("{userid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<User>> GetUserById(string id, CancellationToken cancellationToken)
        {
            var data = await _dietTrackerDbContext.Users.FindAsync(id);
            if (data == null) return new NotFoundResult();

            return data;
        }

        /**
        * Get the current user
        */
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CurrentUser>> GetCurrentUser(CancellationToken cancellationToken)
        {
            var id = _httpContextAccessor.HttpContext.User.Identity.Name;
            var data = await _dietTrackerDbContext.Users
                .Where(user => user.UserId == id)
                .Select(user => new CurrentUser
                {
                    UserId = user.UserId,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    EmailAddress = user.EmailAddress,
                    LastLogin = user.LastLogin,
                    CurrentPlan = user.UserPlans.OrderByDescending(up => up.Start).Select(up => up.Plan).FirstOrDefault(),
                })
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            if (data == null) return new NotFoundResult();

            return data;
        }

        [HttpGet("exists")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<bool>> GetCurrentUserExists(CancellationToken cancellationToken)
        {
            var id = _httpContextAccessor.HttpContext.User.Identity.Name;
            var data = await _dietTrackerDbContext.Users.FindAsync(id);

            if (data == null) return false;

            return true;
        }

    }
}