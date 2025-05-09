using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using LC.ApiKey.Validation;

namespace LC.ApiKey.Attribute;

//Usage: [CustomAuthorization]  
[AttributeUsage(AttributeTargets.Class)]
public class CustomAuthorization : System.Attribute, IAuthorizationFilter
{
    /// <summary>    
    /// This will Authorize User    
    /// </summary>    
    /// <returns></returns>    
    public void OnAuthorization(AuthorizationFilterContext filterContext)
    {

        if (filterContext != null)
        {
            filterContext.HttpContext.Request.Headers.TryGetValue(HeaderNames.Authorization, out StringValues authTokens);
            var _token = authTokens.FirstOrDefault();

            if (_token != null)
            {
                string authToken = _token;
                if (authToken != null)
                {
                    var apiKeyValidator = (IApiKeyValidator)filterContext.HttpContext.RequestServices.GetService(typeof(IApiKeyValidator))!;
                    if (!apiKeyValidator!.IsValid(authToken))
                    {
                        filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        filterContext.Result = new JsonResult("NotAuthorized")
                        {
                            Value = new
                            {
                                Status = "Error",
                                Message = "The Api key is incorrect : Unauthorized access"
                            },
                        };
                        return;
                    }
                }
            }
            else
            {
                //if the request header doesn't contain the authorization header, try to get the API-Key.  
                var key = filterContext.HttpContext.Request.Headers.TryGetValue(Constants.ApiKeyHeaderName, out StringValues apikey);
                var keyvalue = apikey.FirstOrDefault();

                //if the API-Key value is not null. validate the API-Key.  
                var apiKeyValidator = (IApiKeyValidator)filterContext.HttpContext.RequestServices.GetService(typeof(IApiKeyValidator))!;
                if (!apiKeyValidator!.IsValid(keyvalue!))
                {
                    filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                    filterContext.Result = new JsonResult("NotAuthorized")
                    {
                        Value = new
                        {
                            Status = "Error",
                            Message = "Please Provide auth Token"
                        },
                    };
                    return;
                }
            }
        }
    }
}
