using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using LC.ApiKey.Validation;

namespace LC.ApiKey.Attribute;

internal class ApiKeyAuthorizationFilter : IAuthorizationFilter
{
    private readonly IApiKeyValidator _apiKeyValidator;

    public ApiKeyAuthorizationFilter(IApiKeyValidator apiKeyValidator)
    {
        _apiKeyValidator = apiKeyValidator;
    }

    public void OnAuthorization(AuthorizationFilterContext context)
    {
        bool success = context.HttpContext.Request.Headers.TryGetValue
            (Constants.ApiKeyHeaderName, out var apiKeyFromHttpHeader);

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
