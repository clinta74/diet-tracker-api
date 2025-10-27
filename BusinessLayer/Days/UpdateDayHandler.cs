using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.BusinessLayer.Days
{
    public record UpdateDay(DateTime Day, string UserId, CurrentUserDay UserDay) : IRequest<Unit>;

    public class UpdateDayHandler : IRequestHandler<UpdateDay, Unit>
    {
        private readonly DietTrackerDbContext _dbContext;
        public UpdateDayHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<Unit> Handle(UpdateDay request, CancellationToken cancellationToken)
        {
            var data = await _dbContext.UserDays
                .AsNoTracking()
                .Where(userDay => userDay.UserId == request.UserId && userDay.Day == request.Day.Date)
                .FirstOrDefaultAsync(cancellationToken);

            if (data == null)
            {
                _dbContext.UserDays
                    .Add(new UserDay
                    {
                        Day = request.Day.Date,
                        UserId = request.UserId,
                        Water = request.UserDay.Water,
                        Weight = request.UserDay.Weight,
                        Notes = (request.UserDay.Notes == null || request.UserDay.Notes.Trim().Length == 0) ? null : request.UserDay.Notes.Trim(),
                    });
            }
            else
            {
                _dbContext.UserDays.Update(data with
                {
                    Water = request.UserDay.Water,
                    Weight = request.UserDay.Weight,
                    Notes = request.UserDay.Notes == null || request.UserDay.Notes.Trim().Length == 0 ? null : request.UserDay.Notes.Trim(),
                });
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            return Unit.Value;
        }
    }
}