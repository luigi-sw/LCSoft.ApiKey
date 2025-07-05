using LC.ApiKey.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using System.Net.Http.Headers;

namespace LC.ApiKey.Policy.Auhtorization;

internal class ApiKeyAuthorizationHandler : AuthorizationHandler<ApiKeyRequirement>
{
    private readonly IApiKeyValidator _apiKeyValidation;

    public ApiKeyAuthorizationHandler(IApiKeyValidator apiKeyValidation) : base()
    {
        _apiKeyValidation = apiKeyValidation;
    }

    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, ApiKeyRequirement requirement)
    {
        SucceedRequirementIfApiKeyPresentAndValid(context, requirement);
        return Task.CompletedTask;
    }

    internal bool SucceedRequirementIfApiKeyPresentAndValid(AuthorizationHandlerContext context, ApiKeyRequirement requirement)
    {
        if (context.Resource is not HttpContext httpContext)
            return false;

        string? apiKey = null;

        if (httpContext.Request.Headers.TryGetValue(Constants.ApiKeyHeaderName, out StringValues value) &&
        !StringValues.IsNullOrEmpty(value))
        {
            apiKey = value.FirstOrDefault();
        }
        else if (httpContext.Request.Headers.TryGetValue(HeaderNames.Authorization, out var authorizationHeader))
        {
            // Tenta extrair do Authorization: ApiKey xyz
            if (AuthenticationHeaderValue.TryParse(authorizationHeader, out var authHeader))
            {
                if (authHeader.Scheme.Equals(Constants.ApiKeyName, StringComparison.OrdinalIgnoreCase))
                {
                    apiKey = authHeader.Parameter;
                }
            }
        }

        if (string.IsNullOrWhiteSpace(apiKey))
            return false;

        if (!_apiKeyValidation.IsValid(apiKey))
            return false;

        context.Succeed(requirement);
        return true;
    }
}