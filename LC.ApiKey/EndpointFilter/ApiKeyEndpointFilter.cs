using LC.ApiKey.Validation;
using Microsoft.AspNetCore.Http;

namespace LC.ApiKey.EndpointFilter;

public class ApiKeyEndpointFilter(IApiKeyValidator apiKeyValidation) : IEndpointFilter
{
    private readonly IApiKeyValidator _apiKeyValidation = apiKeyValidation;

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        bool success = context.HttpContext.Request.Headers.TryGetValue
            (Constants.ApiKeyHeaderName, out var apiKeyFromHttpHeader);

        if (!success)
        {
            return new UnauthorizedHttpObjectResult("ApiKey is missing.");
        }

        if (string.IsNullOrWhiteSpace(apiKeyFromHttpHeader))
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