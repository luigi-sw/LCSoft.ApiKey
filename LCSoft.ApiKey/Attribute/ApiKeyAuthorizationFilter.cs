using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using Microsoft.Net.Http.Headers;
using LCSoft.ApiKey.Validation;
using LCSoft.ApiKey.Models;

namespace LCSoft.ApiKey.Attribute;

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
        string? apiKey = null;

        var headers = context.HttpContext.Request.Headers;

        bool success = headers.TryGetValue
            (_headerName, out var apiKeyFromHttpHeader);

        if (!success && string.IsNullOrWhiteSpace(apiKeyFromHttpHeader))
        {
            if (headers.TryGetValue(HeaderNames.Authorization, out var authorizationHeader))
            {
                // Tenta extrair do header Authorization com esquema "ApiKey"
                if (AuthenticationHeaderValue.TryParse(authorizationHeader, out var authHeaderValue))
                {
                    if (authHeaderValue.Scheme.Equals(Constants.ApiKeyName, StringComparison.OrdinalIgnoreCase))
                    {
                        apiKey = authHeaderValue.Parameter;
                    }
                }
            } else
            {
                context.Result = new UnauthorizedResult();
                return;
            }
        } else
        {
            apiKey = apiKeyFromHttpHeader.ToString();
        } 

        if (string.IsNullOrWhiteSpace(apiKey))
        {
            context.Result = new UnauthorizedResult();
            return;
        }

        if (!_apiKeyValidator.IsValid(apiKey))
        {
            context.Result = new UnauthorizedResult();
        }
    }
}
