using LC.ApiKey.Policy.Authentication;
using LC.ApiKey.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LC.ApiKey.Extensions;

internal static class ApiKeyAuthenticationExtensions
{
    public static AuthenticationBuilder AddApiKey<TAuthService>(this AuthenticationBuilder builder)
        where TAuthService : class, IApiKeyAuthenticationService
    {
        return AddApiKey<TAuthService>(builder, Constants.Scheme, _ => { });
    }

    public static AuthenticationBuilder AddApiKey<TAuthService>(this AuthenticationBuilder builder, string authenticationScheme)
        where TAuthService : class, IApiKeyAuthenticationService
    {
        return AddApiKey<TAuthService>(builder, authenticationScheme, _ => { });
    }

    public static AuthenticationBuilder AddApiKey<TAuthService>(this AuthenticationBuilder builder, Action<ApiKeyAuthenticationOptions> configureOptions)
        where TAuthService : class, IApiKeyAuthenticationService
    {
        return AddApiKey<TAuthService>(builder, Constants.Scheme, configureOptions);
    }

    public static AuthenticationBuilder AddApiKey<TAuthService>(this AuthenticationBuilder builder, string authenticationScheme, Action<ApiKeyAuthenticationOptions> configureOptions)
        where TAuthService : class, IApiKeyAuthenticationService
    {
        builder.Services.AddSingleton<IPostConfigureOptions<ApiKeyAuthenticationOptions>, ApiKeyAuthenticationPostConfigureOptions>();
        builder.Services.AddTransient<IApiKeyAuthenticationService, TAuthService>();

        return builder.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
            authenticationScheme, configureOptions);
    }
}

