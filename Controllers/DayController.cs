using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.CQRS;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using diet_tracker_api.Models;
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
    public class DayController
    {
        private readonly ILogger<DayController> _logger;
        private readonly DietTrackerDbContext _dietTrackerDbContext;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IMediator _mediator;

        public DayController(ILogger<DayController> logger, DietTrackerDbContext dietTrackerDbContext, IHttpContextAccessor httpContextAccessor, IMediator mediator)
        {
            _logger = logger;
            _dietTrackerDbContext = dietTrackerDbContext;
            _httpContextAccessor = httpContextAccessor;
            _mediator = mediator;
        }

        [HttpGet("{day}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CurrentUserDay>> GetCurrentUserDay(DateTime day, CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext.User.Identity.Name;

            var user = await _dietTrackerDbContext.Users
                .FindAsync(userId);

            if (user == null)
            {
                return new NotFoundObjectResult($"User not found.");
            }

            var data = await _mediator.Send(new Day.GetDay(day, userId));

            return data;
        }

        [HttpPut("{day}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CurrentUserDay>> UpdateCurrentUserDay(DateTime day, UserDay userDay, CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext.User.Identity.Name;

            var user = await _dietTrackerDbContext.Users
                .FindAsync(userId);

            if (user == null)
            {
                return new NotFoundObjectResult($"User not found.");
            }

            using var transaction = _dietTrackerDbContext.Database.BeginTransaction();

            var data = await _dietTrackerDbContext.UserDays
                .Where(userDay => userDay.UserId == userId && userDay.Day == day.Date)
                .AsNoTracking()
                .FirstOrDefaultAsync(cancellationToken);

            if (data == null)
            {
                _dietTrackerDbContext.UserDays
                    .Add(new UserDay
                    {
                        Day = day.Date,
                        UserId = userId,
                        Water = userDay.Water,
                        Weight = userDay.Weight,
                        Condiments = userDay.Condiments,
                        Notes = userDay.Notes,
                    });
            }
            else
            {
                _dietTrackerDbContext.UserDays.Update(data with
                {
                    Water = userDay.Water,
                    Weight = userDay.Weight,
                    Condiments = userDay.Condiments,
                    Notes = userDay.Notes,
                });
            }

            await _dietTrackerDbContext.SaveChangesAsync(cancellationToken);

            _dietTrackerDbContext.UserMeals
                .AddRange(userDay.Meals
                    .Where(m => m.UserMealId == 0)
                    .Select(m => m with
                    {
                        UserId = userId,
                        Day = day.Date,
                    }));


            _dietTrackerDbContext.UserFuelings
                .AddRange(userDay.Fuelings
                    .Where(f => f.UserFuelingId == 0)
                    .Select(f => f with
                    {
                        UserId = userId,
                        Day = day.Date,
                    }));

            _dietTrackerDbContext.UserMeals
                .UpdateRange(userDay.Meals
                    .Where(meal => meal.UserMealId != 0)
                    .Select(meal => _dietTrackerDbContext.UserMeals
                        .AsNoTracking()
                        .First(m => m.UserMealId == meal.UserMealId)
                    with
                    {
                        Name = meal.Name,
                        When = meal.When,
                    }));

            _dietTrackerDbContext.UserFuelings
                .UpdateRange(userDay.Fuelings
                    .Where(fueling => fueling.UserFuelingId != 0)
                    .Select(fueling => _dietTrackerDbContext.UserFuelings
                        .AsNoTracking()
                        .First(f => f.UserFuelingId == fueling.UserFuelingId)
                    with
                    {
                        Name = fueling.Name,
                        When = fueling.When,
                    }));

            // Clean up left over entries not being used.
            var removeMealIds = userDay.Meals.Where(m => m.UserMealId != 0).Select(m => m.UserMealId);

            var removeMeals = _dietTrackerDbContext.UserMeals
                    .Where(userMeal => userMeal.UserId == userId && userMeal.Day == day.Date)
                    .Where(userMeal => !removeMealIds.Contains(userMeal.UserMealId))
                    .AsEnumerable();

            _dietTrackerDbContext.UserMeals.RemoveRange(removeMeals);

            var removeFuelingIds = userDay.Fuelings.Where(fueling => fueling.UserFuelingId != 0).Select(fueling => fueling.UserFuelingId);
            var removeFuelings = _dietTrackerDbContext.UserFuelings
                    .Where(userFueling => userFueling.UserId == userId && userFueling.Day == day.Date)
                    .Where(userFueling => !removeFuelingIds.Contains(userFueling.UserFuelingId))
                    .AsEnumerable();

            _dietTrackerDbContext.UserFuelings.RemoveRange(removeFuelings);

            await _dietTrackerDbContext.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            var result = await _mediator.Send(new Day.GetDay(day, userId));

            return result;
        }
    }
}