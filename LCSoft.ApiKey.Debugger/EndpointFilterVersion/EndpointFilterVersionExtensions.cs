using LCSoft.ApiKey.Extensions;

namespace LCSoft.ApiKey.Debugger.EndpointFilterVersion;

public static class EndpointFilterVersionExtensions
{
    public static IServiceCollection RegisterUsingEndointsFilter(
        this IServiceCollection services)
    {
        services.RegisterApiKeyEndpointFilter();
        return services;
    }
}
