using LCSoft.ApiKey.Extensions;

namespace LCSoft.ApiKey.Debugger.PolicyVersion;

public static class PolicyVersionExtensions
{
    public static IServiceCollection RegisterUsingPolicy(
        this IServiceCollection services)
    {

        services.AddAuthorizationBuilder()
                        .AddPolicy("ApiKeyOrBearer", policy =>
                        {
                            policy.AddAuthenticationSchemes("Bearer", "ApiKey");
                            policy.RequireAuthenticatedUser();
                        });

        services.RegisterApiKeyPolicyAuthorization();
        services.RegisterApiKeyPolicyAuthentication(opts => { });
        return services;
    }

    public static WebApplication UsingPolicy(
        this WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }
}
