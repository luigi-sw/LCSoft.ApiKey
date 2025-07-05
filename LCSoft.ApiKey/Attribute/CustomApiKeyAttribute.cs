using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System.Net.Http.Headers;
using LCSoft.ApiKey.Validation;
using LCSoft.ApiKey.Models;

namespace LCSoft.ApiKey.Attribute;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public class CustomApiKeyAttribute : System.Attribute, IAsyncActionFilter
{
    public string? Header { get; set; }

    public CustomApiKeyAttribute() { }

    public CustomApiKeyAttribute(string header) => Header = header;

    public async Task OnActionExecutionAsync
           (ActionExecutingContext context, ActionExecutionDelegate next)
    {
        string? apiKey = null;

        var services = context.HttpContext.RequestServices;

        var apiKeyValidator = services.GetRequiredService<IApiKeyValidator>();
        var options = services.GetService<IOptions<ApiSettings>>()?.Value;

        string headerName = !string.IsNullOrWhiteSpace(Header)
            ? Header
            : !string.IsNullOrWhiteSpace(options?.HeaderName)
                ? options.HeaderName
                : Constants.ApiKeyHeaderName;

        var headers = context.HttpContext.Request.Headers;

        bool success = headers.TryGetValue
        (headerName, out var apiKeyFromHttpHeader);

        if (!success && string.IsNullOrWhiteSpace(apiKeyFromHttpHeader))
        {
            if (headers.TryGetValue(HeaderNames.Authorization, out var authorizationHeader))
            {
                // Tenta extrair do header Authorization com esquema "ApiKey"
                if (AuthenticationHeaderValue.TryParse(authorizationHeader, out var authHeaderValue))
                {
                    if (authHeaderValue.Scheme.Equals(Constants.ApiKeyName, StringComparison.OrdinalIgnoreCase))
                    {
                        apiKey = authHeaderValue.Parameter;
                    }
                }
            }
            else
            {
                context.Result = new ContentResult()
                {
                    StatusCode = 401,
                    Content = "The Api Key for accessing this endpoint is not available"
                };
                return;
            }
        }
        else
        {
            apiKey = apiKeyFromHttpHeader.ToString();
        }

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            context.Result = new ContentResult()
            {
                StatusCode = 401,
                Content = "The Api key is incorrect : Unauthorized access"
            };
            return;
        }
        
        if (!apiKeyValidator.IsValid(apiKey))
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