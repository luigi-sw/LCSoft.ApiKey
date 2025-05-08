using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace LC.ApiKey.Attribute;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
public class CustomApiKeyAttribute : System.Attribute, IAsyncActionFilter
{
    public async Task OnActionExecutionAsync
           (ActionExecutingContext context, ActionExecutionDelegate next)
    {
        bool success = context.HttpContext.Request.Headers.TryGetValue
            (Constants.ApiKeyName, out var apiKeyFromHttpHeader);
        if (!success)
        {
            context.Result = new ContentResult()
            {
                StatusCode = 401,
                Content = "The Api Key for accessing this endpoint is not available"
            };
            return;
        }
        IConfigurationBuilder configurationBuilder = new ConfigurationBuilder();
        configurationBuilder.AddJsonFile("AppSettings.json");
        IConfiguration Configuration = configurationBuilder.Build();
        string api_key_From_Configuration = Configuration[Constants.ApiKeyName]!;
        if(api_key_From_Configuration == null)
        {
            context.Result = new ContentResult()
            {
                StatusCode = 401,
                Content = "The Api key is incorrect : Unauthorized access"
            };
            return;
        }

        if (!api_key_From_Configuration.Equals(apiKeyFromHttpHeader))
        {
            context.Result = new ContentResult()
            {
                StatusCode = 401,
                Content = "The Api key is incorrect : Unauthorized access"
            };
            return;
        }
        await next();
    }
}