using System;
using System.Linq;
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
    public class DayController
    {
        private readonly ILogger<DayController> _logger;
        private readonly DietTrackerDbContext _dietTrackerDbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public DayController(ILogger<DayController> logger, DietTrackerDbContext dietTrackerDbContext, IHttpContextAccessor httpContextAccessor)
        {
            _logger = logger;
            _dietTrackerDbContext = dietTrackerDbContext;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpGet("{day}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<UserDay>> GetCurrentUserDay(DateTime day)
        {
            var userId = _httpContextAccessor.HttpContext.User.Identity.Name;

            var data = await _dietTrackerDbContext.UserDays
                .Where(userDay => userDay.UserId == userId && userDay.Day == day.Date)
                .Include(userDay => userDay.Fuelings)
                .Include(userDay => userDay.Meals)
                .FirstOrDefaultAsync();

            if (data == null)
            {
                data = new UserDay
                {
                    Day = DateTime.Today,
                    UserId = userId,
                };
            }

            return data;
        }
    }
}