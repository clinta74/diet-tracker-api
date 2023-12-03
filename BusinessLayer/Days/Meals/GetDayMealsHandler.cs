using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.Days.Meals
{
    public record GetDayMeals(DateTime Date, string UserId) : IRequest<IEnumerable<UserDayMeal>>;

    public record UserDayMeal(int UserMealId, string UserId, DateTime Day, string Name, DateTime? When);

    public class GetDayMealsHandler : IRequestHandler<GetDayMeals, IEnumerable<UserDayMeal>>
    {
        private readonly DietTrackerDbContext _dbContext;
        private readonly IMediator _mediator;

        public GetDayMealsHandler(DietTrackerDbContext dbContext, IMediator mediator)
        {
            _dbContext = dbContext;
            _mediator = mediator;
        }

        public async Task<IEnumerable<UserDayMeal>> Handle(GetDayMeals request, CancellationToken cancellationToken)
        {
            var data = await _dbContext.UserMeals
                .Where(userMeal => userMeal.UserId == request.UserId)
                .Where(userMeals => userMeals.Day == request.Date)
                .Select(userMeals => new UserDayMeal(
                    userMeals.UserMealId,
                    userMeals.UserId,
                    userMeals.Day,
                    userMeals.Name,
                    userMeals.When))
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            var plan = await _mediator.Send(new GetCurrentUserPlan(request.UserId));

            var meals = new List<UserDayMeal>(data);
            if (data.Count < plan.MealCount)
            {
                var _meals = new UserDayMeal[plan.MealCount - data.Count];
                Array.Fill(_meals, new UserDayMeal(0, request.UserId, request.Date, "", null));

                meals.AddRange(_meals);
            }

            return meals;
        }
    }
}