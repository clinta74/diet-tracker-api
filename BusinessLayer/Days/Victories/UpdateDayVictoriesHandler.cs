using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.Days.Victories
{
    public record UpdateDayVictories(DateTime Day, string UserId, IEnumerable<Victory> Victories) : IRequest<Unit>;
    public class UpdateDayVictoriesHandler : IRequestHandler<UpdateDayVictories, Unit>
    {
        private readonly DietTrackerDbContext _dbContext;
        public UpdateDayVictoriesHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Unit> Handle(UpdateDayVictories request, CancellationToken cancellationToken)
        {
            using var transaction = _dbContext.Database.BeginTransaction();

            // Handle Victories
            _dbContext.Victories
                .AddRange(request.Victories
                    .Where(victory => victory.VictoryId == 0)
                    .Where(victory => victory.Name.Trim().Length > 0 || victory.When != null)
                    .Select(victory => victory with
                    {
                        UserId = request.UserId,
                        When = request.Day.Date,
                    }));

            await _dbContext.SaveChangesAsync(cancellationToken);

            _dbContext.Victories
                .UpdateRange(request.Victories
                    .Where(victory => victory.VictoryId != 0)
                    .Select(victory => _dbContext.Victories
                        .AsNoTracking()
                        .First(v => v.VictoryId == victory.VictoryId)
                    with
                    {
                        Name = victory.Name,
                    }));

            await _dbContext.SaveChangesAsync(cancellationToken);

            var removeVictoryIds = request.Victories.Where(victory => victory.VictoryId == 0 || (string.IsNullOrWhiteSpace(victory.Name) && victory.VictoryId != 0))
                .Select(victory => victory.VictoryId);

            var removeVictories = _dbContext.Victories
                .Where(victory => victory.UserId == request.UserId && victory.When == request.Day.Date)
                .Where(victory => removeVictoryIds.Contains(victory.VictoryId))
                .AsEnumerable();

            _dbContext.Victories.RemoveRange(removeVictories);

            await _dbContext.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return Unit.Value;
        }
    }
}