using LC.ApiKey.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using System.Net;

namespace LC.ApiKey.Middleware;

public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IApiKeyValidator _apiKeyValidation;
    public ApiKeyMiddleware(RequestDelegate next, 
                            IApiKeyValidator apiKeyValidation)
    {
        _next = next;
        _apiKeyValidation = apiKeyValidation;
    }
    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        if (endpoint != null)
        {
            //For MVC application
            var actionDescriptor = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
            if (actionDescriptor != null)
            {
                var allowAnonymous = actionDescriptor.MethodInfo.GetCustomAttributes(typeof(AllowAnonymousAttribute), true).Length > 0;
                if (allowAnonymous)
                {
                    await _next(context);
                    return;
                }
            } else
            {
                // For minimal API (.AllowAnonymous())
                var allowanonymousAttribute = endpoint.Metadata.GetMetadata<AllowAnonymousAttribute>();
                if (allowanonymousAttribute != null)
                {
                    await _next(context);
                    return;
                }
            }
        }

        // Proceed with API key validation
        bool success = context.Request.Headers.TryGetValue
        (Constants.ApiKeyHeaderName, out var apiKeyFromHttpHeader);

        if (!success)
        {
            context.Response.StatusCode = 401;
            await context.Response.WriteAsync("The Api Key for accessing this endpoint is not available");
            return;
        }

        if (string.IsNullOrWhiteSpace(apiKeyFromHttpHeader))
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return;
        }

        if (!_apiKeyValidation.IsValid(apiKeyFromHttpHeader!))
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return;
        }

        await _next(context);
    }
}
