﻿using LCSoft.ApiKey.Policy.Authentication;
using LCSoft.ApiKey.Validation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace LCSoft.ApiKey.Extensions;

internal static class ApiKeyAuthenticationExtensions
{
    public static AuthenticationBuilder AddApiKey<TAuthService>(this AuthenticationBuilder builder)
        where TAuthService : class, IApiKeyValidator
    {
        return builder.AddApiKey<TAuthService>(ApiKeyAuthenticationOptions.Scheme, _ => { });
    }

    public static AuthenticationBuilder AddApiKey<TAuthService>(this AuthenticationBuilder builder, string authenticationScheme)
        where TAuthService : class, IApiKeyValidator
    {
        return builder.AddApiKey<TAuthService>(authenticationScheme, _ => { });
    }

    public static AuthenticationBuilder AddApiKey<TAuthService>(this AuthenticationBuilder builder, Action<ApiKeyAuthenticationOptions> configureOptions)
        where TAuthService : class, IApiKeyValidator
    {
        return builder.AddApiKey<TAuthService>(ApiKeyAuthenticationOptions.Scheme, configureOptions);
    }

    public static AuthenticationBuilder AddApiKey<TAuthService>(this AuthenticationBuilder builder, string authenticationScheme, Action<ApiKeyAuthenticationOptions> configureOptions)
        where TAuthService : class, IApiKeyValidator
    {
        builder.Services.AddSingleton<IPostConfigureOptions<ApiKeyAuthenticationOptions>, ApiKeyAuthenticationPostConfigureOptions>();
        builder.Services.TryAddTransient<IApiKeyValidator, TAuthService>();

        return builder.AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
            authenticationScheme, configureOptions);
    }
}

