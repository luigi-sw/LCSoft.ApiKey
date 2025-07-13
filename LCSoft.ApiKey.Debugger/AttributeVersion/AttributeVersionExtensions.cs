using LCSoft.ApiKey.Attribute;
using LCSoft.ApiKey.Extensions;

namespace LCSoft.ApiKey.Debugger.AttributeVersion;

public static class AttributeVersionExtensions
{
    public static IServiceCollection RegisterUsingAttributes(
        this IServiceCollection services, bool customAuthorization)
    {
        services.AddControllers(options =>
        {
            if (customAuthorization)
                options.Filters.Add(new CustomAuthorization());
        });

        if (customAuthorization)
            services.RegisterApiKeyCustomAuthorization(applyGlobally: false);
        else
            services.RegisterApiKeyFilterAuthorization();
       
        return services;
    }


}
