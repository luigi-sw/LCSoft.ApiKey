using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using LC.ApiKey.Validation;
using Microsoft.Extensions.Options;
using LC.ApiKey.Models;

namespace LC.ApiKey.Attribute;

internal class ApiKeyAuthorizationFilter : IAuthorizationFilter
{
    private readonly IApiKeyValidator _apiKeyValidator;
    private readonly string _headerName;

    public ApiKeyAuthorizationFilter(IApiKeyValidator apiKeyValidator, IOptions<ApiSettings> options)
    {
        _apiKeyValidator = apiKeyValidator;
        _headerName = string.IsNullOrWhiteSpace(options.Value.HeaderName)
        ? Constants.ApiKeyHeaderName
        : options.Value.HeaderName;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        bool success = context.HttpContext.Request.Headers.TryGetValue
            (_headerName, out var apiKeyFromHttpHeader);

        if (!success)
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        if (string.IsNullOrWhiteSpace(apiKeyFromHttpHeader))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        string apiKey = apiKeyFromHttpHeader.ToString();

        if (!_apiKeyValidator.IsValid(apiKey))
        {
            context.Result = new UnauthorizedResult();
        }
    }
}
