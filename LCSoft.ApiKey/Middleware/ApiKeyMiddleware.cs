using LCSoft.ApiKey.Models;
using LCSoft.ApiKey.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System.Net;
using System.Net.Http.Headers;

namespace LCSoft.ApiKey.Middleware;

public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private readonly IApiKeyValidator _apiKeyValidation;
    private readonly ApiSettings _options;
    public ApiKeyMiddleware(RequestDelegate next, 
                            IApiKeyValidator apiKeyValidation,
                            IOptions<ApiSettings> options)
    {
        _next = next;
        _apiKeyValidation = apiKeyValidation;
        _options = options.Value;
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
            } 
            else
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
        var headerName = !string.IsNullOrWhiteSpace(_options.HeaderName)
            ? _options.HeaderName
            : Constants.ApiKeyHeaderName;

        bool success = context.Request.Headers.TryGetValue(headerName, out var apiKeyFromHttpHeader);
        string? apiKey = null;

        if (!success)
        {
            if (context.Request.Headers.TryGetValue(HeaderNames.Authorization, out var authTokens))
            {
                var authHeaderRaw = authTokens.ToString();
                // Tenta extrair do header Authorization com esquema "ApiKey"
                if (AuthenticationHeaderValue.TryParse(authHeaderRaw, out var authHeaderValue))
                {
                    if (authHeaderValue.Scheme.Equals(Constants.ApiKeyName, StringComparison.OrdinalIgnoreCase))
                    {
                        apiKey = authHeaderValue.Parameter;
                    }
                }
                else
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("The Api Key for accessing this endpoint is not available");
                    return;
                }
            }
            else
            {
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("The Api Key for accessing this endpoint is not available");
                return;
            }
        }
        else
        {
            apiKey = apiKeyFromHttpHeader.ToString();
        }

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return;
        }

        if (!_apiKeyValidation.IsValid(apiKey).IsSuccess)
        {
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            return;
        }

        await _next(context);
    }
}
