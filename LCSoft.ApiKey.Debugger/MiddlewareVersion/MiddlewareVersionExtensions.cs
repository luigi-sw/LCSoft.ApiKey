using LCSoft.ApiKey.Middleware;
using LCSoft.ApiKey.Extensions;

namespace LCSoft.ApiKey.Debugger.MiddlewareVersion;

public static class MiddlewareVersionExtensions
{
    public static IServiceCollection RegisterUsingMiddleware(
        this IServiceCollection services)
    {
        services.RegisterApiKeyMiddleware();
        return services;
    }

    public static WebApplication UsingMiddleware(
        this WebApplication app)
    {
        app.UseMiddleware<ApiKeyMiddleware>();

        return app;
    }
}
