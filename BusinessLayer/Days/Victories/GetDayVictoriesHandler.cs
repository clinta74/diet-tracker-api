using diet_tracker_api.BusinessLayer.Victories;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;

namespace diet_tracker_api.BusinessLayer.Days.Victories;

public record GetDayVictories(DateTime Day, string UserId) : IRequest<IEnumerable<UserDayVictory>>;
public record UserDayVictory(int VictoryId, string? UserId, DateTime Day, string? Name, DateTime? When);
public class GetDayVictoriesHandler(DietTrackerDbContext dbContext, IMediator mediator) : IRequestHandler<GetDayVictories, IEnumerable<UserDayVictory>>
{
    private readonly DietTrackerDbContext _dbContext = dbContext;
    private readonly IMediator _mediator = mediator;

    public async ValueTask<IEnumerable<UserDayVictory>> Handle(GetDayVictories request, CancellationToken cancellationToken)
    {
        var victories =  await _mediator.Send(new GetVictories(request.UserId, VictoryType.NonScale, request.Day));

        return victories.Select(victory => 
            new UserDayVictory(victory.VictoryId, victory.UserId, request.Day, victory.Name, victory.When));
    }
}