using diet_tracker_api.DataLayer;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.Days.Fuelings;

public record GetDayFuelings(DateTime Date, string UserId) : IRequest<IEnumerable<UserDayFueling>>;

public record UserDayFueling(int UserFuelingId, string? UserId, DateTime Day, string? Name, DateTime? When);

public class GetDayFuelingsHandler(DietTrackerDbContext dbContext, IMediator mediator) : IRequestHandler<GetDayFuelings, IEnumerable<UserDayFueling>>
{
    private readonly DietTrackerDbContext _dbContext = dbContext;
    private readonly IMediator _mediator = mediator;

    public async ValueTask<IEnumerable<UserDayFueling>> Handle(GetDayFuelings request, CancellationToken cancellationToken)
    {
        var data = await _dbContext.UserFuelings
            .Where(userFueling => userFueling.UserId == request.UserId)
            .Where(userFueling => userFueling.Day == request.Date)
            .OrderBy(userFueling => userFueling.UserFuelingId)
            .Select(userFueling => new UserDayFueling(
                userFueling.UserFuelingId,
                userFueling.UserId,
                userFueling.Day,
                userFueling.Name,
                userFueling.When))
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var plan = await _mediator.Send(new GetUserPlanForDay(request.UserId, request.Date), cancellationToken);

        // Always return at least the fuelings per day of the plan in effect that day; unused slots are blank placeholders.
        var placeholders = Enumerable.Repeat(
            new UserDayFueling(0, request.UserId, request.Date, "", null),
            Math.Max(0, plan.FuelingCount - data.Count));

        return [.. data, .. placeholders];
    }
}