using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace diet_tracker_api.Filters;

public class SecurityRequirementsOperationFilter : IOperationFilter
{
    /// <summary>
    /// Marks operations that require authorization with the bearer scheme and documents their 401/403 responses.
    /// </summary>
    /// <param name="operation"></param>
    /// <param name="context"></param>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var metadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;

        // Only operations with an 'Authorize' in the chain and no 'AllowAnonymous' need a token.
        if (!metadata.OfType<IAuthorizeData>().Any() || metadata.OfType<IAllowAnonymous>().Any())
        {
            return;
        }

        // add generic message if the controller methods dont already specify the response type
        operation.Responses ??= [];
        operation.Responses.TryAdd("401", new OpenApiResponse { Description = "If Authorization header not present, has no value or no valid jwt bearer token" });
        operation.Responses.TryAdd("403", new OpenApiResponse { Description = "If user not authorized to perform requested action" });

        operation.Security ??= [];
        operation.Security.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", context.Document)] = []
        });
    }
}
