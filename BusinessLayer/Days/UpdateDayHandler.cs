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
            var trimmedNotes = request.UserDay.Notes?.Trim();
            var notes = string.IsNullOrEmpty(trimmedNotes) ? null : trimmedNotes;

            var rowsAffected = await _dbContext.UserDays
                .Where(userDay => userDay.UserId == request.UserId && userDay.Day == request.Day.Date)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(ud => ud.Water, request.UserDay.Water)
                    .SetProperty(ud => ud.Weight, request.UserDay.Weight)
                    .SetProperty(ud => ud.Notes, notes),
                    cancellationToken);

            if (rowsAffected == 0)
            {
                _dbContext.UserDays.Add(new UserDay
                {
                    Day = request.Day.Date,
                    UserId = request.UserId,
                    Water = request.UserDay.Water,
                    Weight = request.UserDay.Weight,
                    Notes = notes,
                });
                
                await _dbContext.SaveChangesAsync(cancellationToken);
            }

            return Unit.Value;
        }
    }
}