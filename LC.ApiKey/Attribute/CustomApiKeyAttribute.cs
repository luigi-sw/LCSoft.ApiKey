using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using LC.ApiKey.Validation;
using LC.ApiKey.Models;
using Microsoft.Extensions.Options;

namespace LC.ApiKey.Attribute;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public class CustomApiKeyAttribute : System.Attribute, IAsyncActionFilter
{
    public string? Header { get; set; }

    public CustomApiKeyAttribute() { }

    public CustomApiKeyAttribute(string header) => Header = header;

    public async Task OnActionExecutionAsync
           (ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var services = context.HttpContext.RequestServices;

        var apiKeyValidator = services.GetRequiredService<IApiKeyValidator>();
        var options = services.GetService<IOptions<ApiSettings>>()?.Value;

        string headerName = !string.IsNullOrWhiteSpace(Header)
            ? Header
            : (!string.IsNullOrWhiteSpace(options?.HeaderName)
                ? options.HeaderName
                : Constants.ApiKeyName);

        bool success = context.HttpContext.Request.Headers.TryGetValue
            (headerName, out var apiKeyFromHttpHeader);
        
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