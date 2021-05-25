using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer.Models;
using diet_tracker_api.Models;
using MediatR;

namespace diet_tracker_api.CQRS.UserDailyTrackings
{
    public record UpsertUserDailyTracking(DateTime Day, string UserId, int UserTrackingId, int Occurance, IEnumerable<CurrentUserDailyTrackingValueRequest> Values) : IRequest<UserDailyTracking>;
    public class UpsertUserDailyTrackingHandler : IRequestHandler<UpsertUserDailyTracking, UserDailyTracking>
    {
        private readonly IMediator _mediator;

        public UpsertUserDailyTrackingHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<UserDailyTracking> Handle(UpsertUserDailyTracking request, CancellationToken cancellationToken)
        {
            var data = await _mediator.Send(new UpdateUserDailyTracking(request.Day, request.UserId, request.UserTrackingId, request.Occurance, request.Values));

            if (data == null)
            {
                return await _mediator.Send(new AddUserDailyTracking(request.Day, request.UserId, request.UserTrackingId, request.Occurance, request.Values));
            }
            else
            {
                return data;
            }
        }
    }
}