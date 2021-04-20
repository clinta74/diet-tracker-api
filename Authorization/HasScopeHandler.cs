using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace diet_tracker_api.Authorization
{
    public class HasScopeHandler : AuthorizationHandler<HasScopeRequirement>
    {
        protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, HasScopeRequirement requirement)
        {
            // If user does not have the scope claim, get out of here
            if (context.User.HasClaim(c => c.Value == requirement.Scope && c.Issuer == requirement.Issuer))
            {
                context.Succeed(requirement);
            }

            return Task.CompletedTask;
        }
    }
}