using LCSoft.ApiKey.Models;
using LCSoft.ApiKey.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System.Net.Http.Headers;

namespace LCSoft.ApiKey.EndpointFilter;

public class ApiKeyEndpointFilter(IApiKeyValidator apiKeyValidation,
                                 IOptions<ApiSettings> options) : IEndpointFilter
{
    private readonly IApiKeyValidator _apiKeyValidation = apiKeyValidation;
    private readonly IOptions<ApiSettings>? _options = options;

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        string? apiKey = null;

        var httpContext = context.HttpContext;
        var headers = httpContext.Request.Headers;

        var headerName = ApiKeyHeaderResolver.Resolve(httpContext, _options);

        if (!httpContext.Request.Headers.TryGetValue(headerName, out var apiKeyFromHttpHeader)
            || string.IsNullOrWhiteSpace(apiKeyFromHttpHeader))
        {
            if (!headers.TryGetValue(HeaderNames.Authorization, out var authTokens))
            {
                // Tenta extrair do header Authorization com esquema "ApiKey"
                if (AuthenticationHeaderValue.TryParse(HeaderNames.Authorization, out var authHeaderValue))
                {
                    if (authHeaderValue.Scheme.Equals(Constants.ApiKeyName, StringComparison.OrdinalIgnoreCase))
                    {
                        apiKey = authHeaderValue.Parameter;
                    }
                }
                else
                {
                    return new UnauthorizedHttpObjectResult("ApiKey is missing.");
                }
            }
            else
            {
                apiKey = authTokens.FirstOrDefault();
            }
        }
        else
        {
            apiKey = apiKeyFromHttpHeader.ToString();
        }

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            return new UnauthorizedHttpObjectResult("ApiKey is invalid.");
        }

        if (!_apiKeyValidation.IsValid(apiKey))
        {
            return new UnauthorizedHttpObjectResult("ApiKey is invalid.");
        }

        return await next(context);
    }
}
