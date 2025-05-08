using Microsoft.AspNetCore.Mvc;

namespace LC.ApiKey.Attribute;

public class ApiKeyAttribute : ServiceFilterAttribute
{
    public ApiKeyAttribute()
        : base(typeof(ApiKeyAuthorizationFilter))
    {
    }
}
