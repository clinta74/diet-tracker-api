using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.BusinessLayer.Victories;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using MediatR;

namespace diet_tracker_api.BusinessLayer.Days.Victories
{
    public record GetDayVictories(DateTime Day, string UserId) : IRequest<IEnumerable<UserDayVictory>>;
    public record UserDayVictory(int VictoryId, string UserId, DateTime Day, string Name, Nullable<DateTime> When);
    public class GetDayVictoriesHandler : IRequestHandler<GetDayVictories, IEnumerable<UserDayVictory>>
    {
        private readonly DietTrackerDbContext _dbContext;
        private readonly IMediator _mediator;

        public GetDayVictoriesHandler(DietTrackerDbContext dbContext, IMediator mediator)
        {
            _dbContext = dbContext;
            _mediator = mediator;
        }

        public async Task<IEnumerable<UserDayVictory>> Handle(GetDayVictories request, CancellationToken cancellationToken)
        {
            var victories =  await _mediator.Send(new GetVictories(request.UserId, VictoryType.NonScale, request.Day));

            return victories.Select(victory => 
                new UserDayVictory(victory.VictoryId, victory.UserId, request.Day, victory.Name, victory.When));
        }
    }
}