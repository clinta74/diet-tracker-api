namespace diet_tracker_api.Extensions;

public static class HttpContextExtensions
{
    /// <summary>
    /// Gets the authenticated user's id. Only call this from endpoints that require authorization.
    /// </summary>
    public static string GetUserId(this HttpContext? context) =>
        context?.User.Identity?.Name ?? throw new InvalidOperationException("The request has no authenticated user id.");
}