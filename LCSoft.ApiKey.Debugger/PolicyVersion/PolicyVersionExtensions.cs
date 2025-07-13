using LCSoft.ApiKey.Extensions;

namespace LCSoft.ApiKey.Debugger.PolicyVersion;

public static class PolicyVersionExtensions
{
    public static IServiceCollection RegisterUsingPolicy(
        this IServiceCollection services)
    {
        services.RegisterApiKeyPolicyAuthorization();
        return services;
    }
    public static IServiceCollection RegisterUsingPolicyAuth(
        this IServiceCollection services,
        IConfiguration configuration)
    {

        services.RegisterApiKeyPolicyAuthentication(opts =>
        {
            // You need to set a header, cannot be 'Authorization' because
            // it is reserved for format like ApiKey <your_api_key>.
            // There is a validation from microsoft on the format in this header.
            // the default value will be 'Authorization' due it's the fallback.
            opts.HeaderName = "X-API-Key";
        });
        return services;
    }
}
