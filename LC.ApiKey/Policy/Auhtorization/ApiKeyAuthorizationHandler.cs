using LC.ApiKey.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

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
        var result = SucceedRequirementIfApiKeyPresentAndValid(context, requirement);

        if (result)
        {
            return Task.CompletedTask;
        } else
        {
            context.Fail();
            return Task.CompletedTask;
        }   
    }

    internal bool SucceedRequirementIfApiKeyPresentAndValid(AuthorizationHandlerContext context, ApiKeyRequirement requirement)
    {
        if (context.Resource is HttpContext httpContext)
        {
            if (!httpContext.Request.Headers.TryGetValue(Constants.ApiKeyHeaderName, out StringValues value))
            {
                context.Fail();
                return false;
            }

            var apiKey = value.FirstOrDefault();

            if (string.IsNullOrWhiteSpace(apiKey))
            {
                context.Fail();
                return false;
            }


            if (_apiKeyValidation.IsValid(apiKey))
            {
                context.Succeed(requirement);
                return true;
            }

            if (requirement.ApiKeys is not null)
            {
                if (apiKey != null && requirement.ApiKeys.Any(requiredApiKey => apiKey == requiredApiKey))
                {
                    context.Succeed(requirement);
                    return true;
                }
            }
        }
        return false;
    }
}