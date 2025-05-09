using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using LC.ApiKey.Validation;

namespace LC.ApiKey.Attribute;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public class CustomApiKeyAttribute : System.Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync
           (ActionExecutingContext context, ActionExecutionDelegate next)
    {
        bool success = context.HttpContext.Request.Headers.TryGetValue
            (Constants.ApiKeyName, out var apiKeyFromHttpHeader);
        if (!success)
        {
            context.Result = new ContentResult()
            {
                StatusCode = 401,
                Content = "The Api Key for accessing this endpoint is not available"
            };
            return;
        }

        if (string.IsNullOrWhiteSpace(apiKeyFromHttpHeader))
        {
            context.Result = new ContentResult()
            {
                StatusCode = 401,
                Content = "The Api key is incorrect : Unauthorized access"
            };
            return;
        }

        var apiKeyValidator = context.HttpContext.RequestServices.GetRequiredService<IApiKeyValidator>();
        if (!apiKeyValidator.IsValid(apiKeyFromHttpHeader!))
        {
            context.Result = new ContentResult()
            {
                StatusCode = 401,
                Content = "The Api key is incorrect : Unauthorized access"
            };
            return;
        }

        await next();
    }
}