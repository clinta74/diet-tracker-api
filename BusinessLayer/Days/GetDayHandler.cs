using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.Days
{
    public record CurrentUserDay
    {
        public string UserId { get; init; }
        public DateTime Day { get; init; }
        public int Water { get; init; }
        public decimal Weight { get; init; }
        public decimal CumulativeWeightChange { get; init; }
        public decimal WeightChange { get; init; }
        public string Notes { get; init; }
    }

    public record GetDay(DateTime Date, string UserId) : IRequest<CurrentUserDay>;
    public class GetDayHandler : IRequestHandler<GetDay, CurrentUserDay>
    {
        private readonly DietTrackerDbContext _dbContext;
        private readonly IMediator _mediator;

        public GetDayHandler(DietTrackerDbContext dbContext, IMediator mediator)
        {
            _dbContext = dbContext;
            _mediator = mediator;
        }

        public async Task<CurrentUserDay> Handle(GetDay request, CancellationToken cancellationToken)
        {
            var data = await _dbContext.UserDays
                .Where(userDay => userDay.UserId == request.UserId)
                .Where(userDay => userDay.Day == request.Date)
                .Select(userDay => new CurrentUserDay
                {
                    Day = userDay.Day,
                    UserId = userDay.UserId,
                    Water = userDay.Water,
                    Weight = userDay.Weight,
                    Notes = userDay.Notes
                })
                .AsNoTracking()
                .SingleOrDefaultAsync(cancellationToken);

            if (data == null)
            {
                data = new CurrentUserDay
                {
                    Day = request.Date,
                    UserId = request.UserId,
                    Water = 0,
                    Weight = 0,
                    Notes = null,
                };
            }

            var weights = await _dbContext.UserDays
                .Where(userDay => userDay.UserId == request.UserId && userDay.Weight > 0)
                .OrderBy(userDay => userDay.Day)
                .Where(userDay => userDay.Day <= request.Date)
                .AsNoTracking()
                .Select(userDay => userDay.Weight)
                .ToListAsync(cancellationToken);

            if (weights.Count > 1)
            {
                return data with
                {
                    CumulativeWeightChange = weights.First() - weights.Last(),
                    WeightChange = weights[weights.Count - 2] - weights.Last(),
                };
            }

            if (weights.Count > 0)
            {
                return data with
                {
                    CumulativeWeightChange = weights.First() - weights.Last(),
                };
            }

            return data;
        }
    }
}