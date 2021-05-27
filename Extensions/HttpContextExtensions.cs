using Microsoft.AspNetCore.Http;

namespace diet_tracker_api.Extensions
{
    public static class HttpContextExtensions
    {
        public static string GetUserId(this HttpContext context) => context.User.Identity.Name;
    }
}