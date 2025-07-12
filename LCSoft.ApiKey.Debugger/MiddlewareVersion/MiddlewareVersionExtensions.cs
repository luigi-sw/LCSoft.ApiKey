using LCSoft.ApiKey.Middleware;
using LCSoft.ApiKey.Extensions;

namespace LCSoft.ApiKey.Debugger.MiddlewareVersion;

public static class MiddlewareVersionExtensions
{
    public static IServiceCollection RegisterUsingMiddleware(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.RegisterApiKeyMiddleware(configuration, opts => { });
        return services;
    }

    public static WebApplication UsingMiddleware(
        this WebApplication app)
    {
        app.UseMiddleware<ApiKeyMiddleware>();

        return app;
    }
}
