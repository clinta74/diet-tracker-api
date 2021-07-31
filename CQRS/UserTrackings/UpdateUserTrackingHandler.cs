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
    public record UpdateUserTracking(
        string UserId, 
        int UserTrackingId, 
        string Title, 
        string Description, 
        int Occurrences, 
        bool Disabled,
        IEnumerable<UserTrackingValue> Values) : IRequest<UserTracking>;
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

            await _dbContext.SaveChangesAsync(cancellationToken);

            _dbContext.UserTrackingValues
                .AddRange(request.Values
                    .Where(userTrackingValue => userTrackingValue.UserTrackingValueId == 0)
                    .Select(userTrackingValue => new UserTrackingValue
                    {
                        UserTrackingId = data.UserTrackingId,
                        Name = userTrackingValue.Name,
                        Description = userTrackingValue.Description,
                        Disabled = userTrackingValue.Disabled,
                        Order = userTrackingValue.Order,
                        Type = userTrackingValue.Type,
                        Metadata = userTrackingValue.Metadata,
                    })
                );

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
                    .FirstOrDefaultAsync(cancellationToken);

                _dbContext.Update(value with
                {
                    Name = userTrackingValue.Name,
                    Description = userTrackingValue.Description,
                    Order = userTrackingValue.Order,
                    Type = userTrackingValue.Type,
                    Disabled = userTrackingValue.Disabled,
                    Metadata = userTrackingValue.Metadata,
                });
            }

            var removeTrackingValueIds = request.Values.Where(value => value.UserTrackingValueId != 0).Select(value => value.UserTrackingValueId);
            var removeTrackingValues = _dbContext.UserTrackingValues
                    .Where(userTrackingValue => userTrackingValue.Tracking.UserId == request.UserId)
                    .Where(userTrackingValue => userTrackingValue.UserTrackingId == request.UserTrackingId)
                    .Where(userTrackingValue => !removeTrackingValueIds.Contains(userTrackingValue.UserTrackingValueId))
                    .AsEnumerable();

            _dbContext.UserTrackingValues
                .RemoveRange(removeTrackingValues);

            await _dbContext.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return data;
        }
    }
}