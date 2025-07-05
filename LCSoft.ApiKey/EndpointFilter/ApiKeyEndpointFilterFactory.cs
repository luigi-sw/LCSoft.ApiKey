using LCSoft.ApiKey.Models;
using LCSoft.ApiKey.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System.Net.Http.Headers;

namespace LCSoft.ApiKey.EndpointFilter;

public static class ApiKeyEndpointFilterFactory
{
    public static Func<EndpointFilterFactoryContext, EndpointFilterDelegate, EndpointFilterDelegate> CreateFactory(string? overrideHeaderName = null)
    {
        return (factoryContext, next) =>
        {
            return async invocationContext =>
            {
                string? apiKey = null;

                var services = invocationContext.HttpContext.RequestServices;

                var validator = services.GetRequiredService<IApiKeyValidator>();
                var options = services.GetRequiredService<IOptions<ApiSettings>>();

                var headerName = ApiKeyHeaderResolver.Resolve(
                    invocationContext.HttpContext, options, overrideHeaderName);

                var request = invocationContext.HttpContext.Request;

                if (!request.Headers.TryGetValue(headerName, out var apiKeyFromHttpHeader)
                    || string.IsNullOrWhiteSpace(apiKeyFromHttpHeader))
                {
                    if (!request.Headers.TryGetValue(HeaderNames.Authorization, out var authTokens))
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

                if (!validator.IsValid(apiKey))
                {
                    return new UnauthorizedHttpObjectResult("ApiKey is invalid.");
                }

                return await next(invocationContext);
            };
        };
    }
}
