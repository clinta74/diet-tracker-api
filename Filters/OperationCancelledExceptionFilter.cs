using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace diet_tracker_api.Filters;

public class OperationCancelledExceptionFilter(ILoggerFactory loggerFactory) : ExceptionFilterAttribute
{
    private readonly ILogger _logger = loggerFactory.CreateLogger<OperationCancelledExceptionFilter>();

    public override void OnException(ExceptionContext context)
    {
        if (context.Exception is OperationCanceledException)
        {
            _logger.LogInformation("Request was cancelled");
            context.ExceptionHandled = true;
            context.Result = new StatusCodeResult(499);
        }
    }
}