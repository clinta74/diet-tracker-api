using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.CQRS.UserTrackings
{
    public record UpdateUserTracking(string UserId, int UserTrackingId, string Title, string Description, int Occurrences, bool Disabled, IEnumerable<UserTrackingValue> Values) : IRequest<UserTracking>;
    public class UpdateUserTrackingHandler : IRequestHandler<UpdateUserTracking, UserTracking>
    {
        private readonly DietTrackerDbContext _dbContext;

        public UpdateUserTrackingHandler(DietTrackerDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<UserTracking> Handle(UpdateUserTracking request, CancellationToken cancellationToken)
        {
            var data = await _dbContext.UserTrackings
                        .AsNoTracking()
                        .Where(u => u.UserTrackingId.Equals(request.UserTrackingId))
                        .Where(u => u.UserId.Equals(request.UserId))
                        .SingleOrDefaultAsync(cancellationToken);

            if (data == null)
            {
                throw new ArgumentException($"UserTrackingId ({request.UserTrackingId}) not found.");
            }

            using var transaction = _dbContext.Database.BeginTransaction();
            _dbContext.UserTrackings
                .Update(data with
                {
                    Title = request.Title,
                    Description = request.Description,
                    Occurrences = request.Occurrences,
                    Disabled = request.Disabled
                });

            await _dbContext.SaveChangesAsync();

            _dbContext.UserTrackingValues
                .AddRange(request.Values
                    .Where(v => v.UserTrackingValueId == 0)
                    .Select(v => new UserTrackingValue
                    {

                    }));

            _dbContext.UserTrackingValues
                .AddRange(request.Values
                    .Where(v => v.UserTrackingValueId == 0)
                    .Select(v => new UserTrackingValue
                    {
                        Name = v.Name,
                        Description = v.Description,
                        Disabled = v.Disabled,
                        Order = v.Order,
                        Type = v.Type,
                    }));

            await _dbContext.SaveChangesAsync();

            var values = await _dbContext.UserTrackingValues
                .AsNoTracking()
                .Where(u => u.Tracking.UserTrackingId.Equals(request.UserTrackingId))
                .Where(u => u.Tracking.UserId.Equals(request.UserId))
                .ToListAsync(cancellationToken);

            foreach (var userTrackingValue in request.Values.Where(v => v.UserTrackingValueId != 0))
            {
                var value = await _dbContext.UserTrackingValues
                    .AsNoTracking()
                    .Where(p => p.UserTrackingValueId == userTrackingValue.UserTrackingValueId)
                    .Where(p => p.Tracking.UserId == request.UserId)
                    .FirstOrDefaultAsync();

                _dbContext.Update(value with
                {
                    Name = userTrackingValue.Name,
                    Description = userTrackingValue.Description,
                    Order = userTrackingValue.Order,
                    Type = userTrackingValue.Type,
                    Disabled = userTrackingValue.Disabled
                });

                await _dbContext.SaveChangesAsync();
            }

            _dbContext.UserTrackingValues
                .RemoveRange(request.Values
                    .Where(v => request.Values
                        .Select(r => r.UserTrackingValueId)
                        .Contains(v.UserTrackingValueId))
                    .ToList()
                );

            await _dbContext.SaveChangesAsync();

            await transaction.CommitAsync();

            return data;
        }
    }
}