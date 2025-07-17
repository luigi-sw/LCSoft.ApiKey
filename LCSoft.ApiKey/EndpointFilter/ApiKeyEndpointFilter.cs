using LCSoft.ApiKey.Models;
using LCSoft.ApiKey.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System.Net.Http.Headers;

namespace LCSoft.ApiKey.EndpointFilter;

public class ApiKeyEndpointFilter : IEndpointFilter
{
    private readonly IApiKeyValidator _apiKeyValidation;
    private readonly IOptions<ApiSettings>? _options;

    public ApiKeyEndpointFilter(IApiKeyValidator apiKeyValidation,
                                     IOptions<ApiSettings> options)
    {
        _apiKeyValidation = apiKeyValidation;
        _options = options;
    }

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        string? apiKey = null;

        var httpContext = context.HttpContext;
        var headers = httpContext.Request.Headers;

        var headerName = ApiKeyHeaderResolver.Resolve(httpContext, _options);

        if (!httpContext.Request.Headers.TryGetValue(headerName, out var apiKeyFromHttpHeader)
            || string.IsNullOrWhiteSpace(apiKeyFromHttpHeader))
        {
            if (headers.TryGetValue(HeaderNames.Authorization, out var authTokens))
            {
                var rawAuthorization = authTokens.FirstOrDefault();
                // Tenta extrair do header Authorization com esquema "ApiKey"
                if (!string.IsNullOrWhiteSpace(rawAuthorization) &&
                    AuthenticationHeaderValue.TryParse(rawAuthorization, out var authHeaderValue))
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
                return new UnauthorizedHttpObjectResult("ApiKey is missing.");
            }
        }
        else
        {
            apiKey = apiKeyFromHttpHeader.ToString();
        }

        if (!_apiKeyValidation.IsValid(apiKey!).IsSuccess)
        {
            return new UnauthorizedHttpObjectResult("ApiKey is invalid.");
        }

        return await next(context);
    }
}
