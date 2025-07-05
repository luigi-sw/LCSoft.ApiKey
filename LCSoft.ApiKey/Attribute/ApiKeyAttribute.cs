using Microsoft.AspNetCore.Mvc;

namespace LCSoft.ApiKey.Attribute;

public class ApiKeyAttribute : ServiceFilterAttribute
{
    public ApiKeyAttribute()
        : base(typeof(ApiKeyAuthorizationFilter))
    {
    }
}
