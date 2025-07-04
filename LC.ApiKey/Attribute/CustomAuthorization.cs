using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using LC.ApiKey.Validation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using LC.ApiKey.Models;

namespace LC.ApiKey.Attribute;

//Usage: [CustomAuthorization(AuthorizationHeader = "X-Auth", ApiKeyHeader = "X-API-Key")] 
[AttributeUsage(AttributeTargets.Class)]
public class CustomAuthorization : System.Attribute, IAuthorizationFilter
{
    public string? AuthorizationHeader { get; set; }
    public string? ApiKeyHeader { get; set; }
  
    public void OnAuthorization(AuthorizationFilterContext filterContext)
    {

        if (filterContext == null) return;

        var services = filterContext.HttpContext.RequestServices;

        var apiKeyValidator = services.GetRequiredService<IApiKeyValidator>();
        var options = services.GetService<IOptions<ApiSettings>>()?.Value;

        string authorizationHeader = !string.IsNullOrWhiteSpace(AuthorizationHeader)
            ? AuthorizationHeader
            : HeaderNames.Authorization;

        string apiKeyHeader = !string.IsNullOrWhiteSpace(ApiKeyHeader)
            ? ApiKeyHeader
            : (!string.IsNullOrWhiteSpace(options?.HeaderName)
                ? options.HeaderName
                : Constants.ApiKeyHeaderName);

        filterContext.HttpContext.Request.Headers.TryGetValue(authorizationHeader, out var authTokens);
        var authToken = authTokens.FirstOrDefault();

        if (!string.IsNullOrWhiteSpace(authToken))
        {
            if (!apiKeyValidator!.IsValid(authToken))
            {
                SetForbiddenResult(filterContext, "The Api key is incorrect : Unauthorized access");
                return;
            }
        }
        else
        {
            //if the request header doesn't contain the authorization header, try to get the API-Key.
            filterContext.HttpContext.Request.Headers.TryGetValue(apiKeyHeader, out var apiKeyTokens);
            var apiKeyValue = apiKeyTokens.FirstOrDefault();

            //if the API-Key value is not null. validate the API-Key.  
            if (string.IsNullOrWhiteSpace(apiKeyValue) || !apiKeyValidator.IsValid(apiKeyValue!))
            {
                SetForbiddenResult(filterContext, "Please Provide valid auth Token");
                return;
            }
        }
    }

    private static void SetForbiddenResult(AuthorizationFilterContext context, string message)
    {
        context.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
        context.Result = new JsonResult("NotAuthorized")
        {
            Value = new
            {
                Status = "Error",
                Message = message
            },
        };
    }
}
