using LCSoft.ApiKey.Attribute;

namespace LCSoft.ApiKey.Debugger.AttributeVersion;

public static class AttributeVersionExtensions
{
    public static IServiceCollection RegisterUsingAttributes(
        this IServiceCollection services)
    {
        services.AddControllers(options =>
        {
            options.Filters.Add(new CustomAuthorization());
        });
        return services;
    }


}
