using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using System.Net;

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
            //get the authorization header  
            Microsoft.Extensions.Primitives.StringValues authTokens;
            filterContext.HttpContext.Request.Headers.TryGetValue("Authorization", out authTokens);

            var _token = authTokens.FirstOrDefault();

            if (_token != null)
            {
                string authToken = _token;
                if (authToken != null)
                {
                    if (IsValidToken(authToken))
                    {
                        filterContext.HttpContext.Response.Headers.Add("Authorization", authToken);
                        filterContext.HttpContext.Response.Headers.Add("AuthStatus", "Authorized");

                        filterContext.HttpContext.Response.Headers.Add("storeAccessiblity", "Authorized");

                        return;
                    }
                    else
                    {
                        filterContext.HttpContext.Response.Headers.Add("Authorization", authToken);
                        filterContext.HttpContext.Response.Headers.Add("AuthStatus", "NotAuthorized");

                        filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.Forbidden;
                        filterContext.HttpContext.Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = "Not Authorized";
                        filterContext.Result = new JsonResult("NotAuthorized")
                        {
                            Value = new
                            {
                                Status = "Error",
                                Message = "Invalid Token"
                            },
                        };
                    }

                }

            }
            else
            {
                //if the request header doesn't contain the authorization header, try to get the API-Key.  
                Microsoft.Extensions.Primitives.StringValues apikey;
                var key = filterContext.HttpContext.Request.Headers.TryGetValue("ApiKey", out apikey);
                var keyvalue = apikey.FirstOrDefault();

                //if the API-Key value is not null. validate the API-Key.  
                if (keyvalue != null)
                {
                    filterContext.HttpContext.Response.Headers.Add("ApiKey", keyvalue);
                    filterContext.HttpContext.Response.Headers.Add("AuthStatus", "Authorized");

                    filterContext.HttpContext.Response.Headers.Add("storeAccessiblity", "Authorized");

                    return;
                }


                filterContext.HttpContext.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                filterContext.HttpContext.Response.HttpContext.Features.Get<IHttpResponseFeature>().ReasonPhrase = "Please Provide authToken";
                filterContext.Result = new JsonResult("Please Provide auth Token")
                {
                    Value = new
                    {
                        Status = "Error",
                        Message = "Please Provide auth Token"
                    },
                };
            }
        }
    }

    public bool IsValidToken(string authToken)
    {
        //validate Token here    
        return true;
    }
}
