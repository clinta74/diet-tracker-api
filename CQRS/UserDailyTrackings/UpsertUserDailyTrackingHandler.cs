using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.DataLayer;
using diet_tracker_api.DataLayer.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.CQRS.UserDailyTrackings
{
    public record UpsertUserDailyTracking(DateTime Day, string UserId, int UserTrackingId, int Occurance, int Value, DateTime When) : IRequest<UserDailyTracking>;
    public class UpsertUserDailyTrackingHandler : IRequestHandler<UpsertUserDailyTracking, UserDailyTracking>
    {
        private readonly IMediator _mediator;

        public UpsertUserDailyTrackingHandler(IMediator mediator)
        {
            _mediator = mediator;
        }

        public async Task<UserDailyTracking> Handle(UpsertUserDailyTracking request, CancellationToken cancellationToken)
        {
            var data = await _mediator.Send(new UpdateUserDailyTracking(request.Day, request.UserId, request.UserTrackingId, request.Occurance, request.Value, request.When));

            if (data == null)
            {
                return await _mediator.Send(new AddUserDailyTracking(request.Day, request.UserId, request.UserTrackingId, request.Occurance, request.Value, request.When));
            }
            else
            {
                return data;
            }
        }
    }
}