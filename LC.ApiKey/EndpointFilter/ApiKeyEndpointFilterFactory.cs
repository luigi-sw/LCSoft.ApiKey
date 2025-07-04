using LC.ApiKey.Models;
using LC.ApiKey.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LC.ApiKey.EndpointFilter;

public static class ApiKeyEndpointFilterFactory
{
    public static Func<EndpointFilterFactoryContext, EndpointFilterDelegate, EndpointFilterDelegate> CreateFactory(string? overrideHeaderName = null)
    {
        return (factoryContext, next) =>
        {
            return async invocationContext =>
            {
                var services = invocationContext.HttpContext.RequestServices;

                var validator = services.GetRequiredService<IApiKeyValidator>();
                var options = services.GetRequiredService<IOptions<ApiSettings>>();

                var headerName = ApiKeyHeaderResolver.Resolve(
                    invocationContext.HttpContext, options, overrideHeaderName);

                var request = invocationContext.HttpContext.Request;

                if (!request.Headers.TryGetValue(headerName, out var apiKey) || string.IsNullOrWhiteSpace(apiKey))
                {
                    return new UnauthorizedHttpObjectResult("ApiKey is missing.");
                }

                if (!validator.IsValid(apiKey!))
                {
                    return new UnauthorizedHttpObjectResult("ApiKey is invalid.");
                }

                return await next(invocationContext);
            };
        };
    }
}
