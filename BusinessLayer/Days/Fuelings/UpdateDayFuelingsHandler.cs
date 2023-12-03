using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.Days.Fuelings
{
    public record UpdateDayFuelings(DateTime Day, string UserId, IEnumerable<UserFueling> Fuelings) : IRequest<Unit>;
    public class UpdateDayFuelingsHandler : IRequestHandler<UpdateDayFuelings, Unit>
    {
        private readonly DietTrackerDbContext _dbContext;
        public UpdateDayFuelingsHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Unit> Handle(UpdateDayFuelings request, CancellationToken cancellationToken)
        {
            using var transaction = _dbContext.Database.BeginTransaction();
            // Handle Fuelings
            _dbContext.UserFuelings
               .AddRange(request.Fuelings
                   .Where(f => f.UserFuelingId == 0)
                   .Where(f => f.Name.Trim().Length > 0 || f.When != null)
                   .Select(f => f with
                   {
                       UserId = request.UserId,
                       Day = request.Day.Date,
                   }));

            await _dbContext.SaveChangesAsync(cancellationToken);

            _dbContext.UserFuelings
                 .UpdateRange(request.Fuelings
                     .Where(fueling => fueling.UserFuelingId != 0)
                     .Select(fueling => _dbContext.UserFuelings
                         .AsNoTracking()
                         .First(f => f.UserFuelingId == fueling.UserFuelingId)
                     with
                     {
                         Name = fueling.Name,
                         When = fueling.When,
                     }));

            await _dbContext.SaveChangesAsync(cancellationToken);

            var removeFuelingIds = request.Fuelings.Where(fueling => fueling.UserFuelingId == 0 || (string.IsNullOrWhiteSpace(fueling.Name) && fueling.UserFuelingId != 0))
                .Select(fueling => fueling.UserFuelingId);

            var removeFuelings = _dbContext.UserFuelings
                .Where(userFueling => userFueling.UserId == request.UserId && userFueling.Day == request.Day.Date)
                .Where(userFueling => removeFuelingIds.Contains(userFueling.UserFuelingId))
                .AsEnumerable();

            _dbContext.UserFuelings.RemoveRange(removeFuelings);

            await _dbContext.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return Unit.Value;
        }
    }
}