using LC.ApiKey.Models;
using LC.ApiKey.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace LC.ApiKey.EndpointFilter;

public class ApiKeyEndpointFilter(IApiKeyValidator apiKeyValidation,
                                 IOptions<ApiSettings> options) : IEndpointFilter
{
    private readonly IApiKeyValidator _apiKeyValidation = apiKeyValidation;
    private readonly IOptions<ApiSettings>? _options = options;

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var httpContext = context.HttpContext;

        var headerName = ApiKeyHeaderResolver.Resolve(httpContext, _options);

        if (!httpContext.Request.Headers.TryGetValue(headerName, out var apiKeyFromHttpHeader)
            || string.IsNullOrWhiteSpace(apiKeyFromHttpHeader))
        {
            return new UnauthorizedHttpObjectResult("ApiKey is missing.");
        }

        string apiKey = apiKeyFromHttpHeader.ToString();

        if (!_apiKeyValidation.IsValid(apiKey))
        {
            return new UnauthorizedHttpObjectResult("ApiKey is invalid.");
        }

        return await next(context);
    }
}
