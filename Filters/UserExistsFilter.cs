using diet_tracker_api.BusinessLayer.Users;
using diet_tracker_api.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace diet_tracker_api.Filters;

public class UserExistsFilter(IMediator mediator) : ActionFilterAttribute
{
    private readonly IMediator _mediator = mediator;

    public override async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var userId = context.HttpContext.GetUserId();

        if (!await _mediator.Send(new UserExists(userId)))
        {
            context.Result = new NotFoundObjectResult($"User not found.");
            return;
        }

        await base.OnActionExecutionAsync(context, next);
    }
}