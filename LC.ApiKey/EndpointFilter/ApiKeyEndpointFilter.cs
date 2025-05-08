using LC.ApiKey.Validation;
using Microsoft.AspNetCore.Http;

namespace LC.ApiKey.EndpointFilter;

public class ApiKeyEndpointFilter(IApiKeyValidator apiKeyValidation) : IEndpointFilter
{
    private readonly IApiKeyValidator _apiKeyValidation = apiKeyValidation;

    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        //string? apiKey = context.HttpContext.Request.Headers[Constants.ApiKeyHeaderName];
        bool success = context.HttpContext.Request.Headers.TryGetValue
            (Constants.ApiKeyHeaderName, out var apiKeyFromHttpHeader);

        if (!success)
        {
           // return new UnauthorizedHttpObjectResult(AuthConstants.ApiKeyMissing);
            return Results.Unauthorized();
        }

        if (string.IsNullOrWhiteSpace(apiKeyFromHttpHeader))
        {
            return Results.Unauthorized();
        }

        string apiKey = apiKeyFromHttpHeader.ToString();

        if (!_apiKeyValidation.IsValid(apiKey))
        {
            return Results.Unauthorized();
        }

        return await next(context);
    }
}