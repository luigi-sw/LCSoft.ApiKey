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
        //string? apiKey = context.HttpContext.Request.Headers[Constants.ApiKeyHeaderName];
        //string userApiKey = context.HttpContext.Request.Headers[Constants.ApiKeyHeaderName].ToString();

        bool success = context.HttpContext.Request.Headers.TryGetValue
            (Constants.ApiKeyHeaderName, out var apiKeyFromHttpHeader);

        if (!success)
        {
            //context.Result = new UnauthorizedObjectResult(AuthConstants.ApiKeyInvalid);
            context.Result = new ContentResult()
            {
                StatusCode = 401,
                Content = "The Api Key for accessing this endpoint is not available"
            };
            return;
        }

        if (string.IsNullOrWhiteSpace(apiKeyFromHttpHeader))
        {
            context.Result = new BadRequestResult();
            return;
        }

        string apiKey = apiKeyFromHttpHeader.ToString();

        if (!_apiKeyValidator.IsValid(apiKey))
        {
            context.Result = new UnauthorizedResult();
        }
    }
}
