using LCSoft.ApiKey.Attribute;
#if NET7_0_OR_GREATER
using LCSoft.ApiKey.EndpointFilter;
#endif
using LCSoft.ApiKey.Middleware;
using LCSoft.ApiKey.Models;
using LCSoft.ApiKey.Policy.Auhtorization;
using LCSoft.ApiKey.Policy.Authentication;
using LCSoft.ApiKey.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LCSoft.ApiKey.Extensions;

public static class ApiKeyExtensions
{
    #if NET6_0
        public const string BearerScheme = "Bearer";
    #endif

    public static IServiceCollection RegisterApiKeyFilterAuthorization(
        this IServiceCollection services,
        Action<ApiSettings> configureOptions,
        string sectionName = Constants.ApiKeyName) =>
            services.RegisterApiKeyFilterAuthorization(null, configureOptions, sectionName);

    public static IServiceCollection RegisterApiKeyFilterAuthorization(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = Constants.ApiKeyName) =>
            services.RegisterApiKeyFilterAuthorization(configuration, null, sectionName);

    public static IServiceCollection RegisterApiKeyFilterAuthorization(this IServiceCollection services,
        IConfiguration? configuration = null,
        Action<ApiSettings>? configureOptions = null,
        string sectionName = Constants.ApiKeyName)
    {
        services.AddScoped<ApiKeyAuthorizationFilter>();
        services.RegisterApikeyServices(configuration, configureOptions, sectionName);
        return services;
    }

    public static IServiceCollection RegisterApiKeyCustomAuthorization(
       this IServiceCollection services,
       IConfiguration? configuration = null,
       Action<ApiSettings>? configureOptions = null,
       string sectionName = Constants.ApiKeyName,
       bool applyGlobally = false)
    {
        services.RegisterApikeyServices(configuration, configureOptions, sectionName);

        services.AddScoped<CustomAuthorization>();

        if (applyGlobally)
        {
            services.Configure<MvcOptions>(options =>
            {
                options.Filters.AddService<CustomAuthorization>();
            });
        }

        return services;
    }

    public static IServiceCollection RegisterApiKeyPolicyAuthorization(
        this IServiceCollection services,
        IConfiguration? configuration = null,
        Action<ApiSettings>? configureOptions = null,
        string sectionName = Constants.ApiKeyName)
    {
        #if NET7_0_OR_GREATER
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer();

        services.AddAuthorizationBuilder().AddDefaultPolicy("", policy =>
        {
            policy.AddAuthenticationSchemes(new[] { Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerDefaults.AuthenticationScheme });
            policy.Requirements.Add(new ApiKeyRequirement());
        });

        #else
        services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = BearerScheme;
                    options.DefaultChallengeScheme = BearerScheme;
                })
                .AddJwtBearer();

                services.AddAuthorization(options =>
                {
                    options.DefaultPolicy = new AuthorizationPolicyBuilder(BearerScheme)
                        .AddRequirements(new ApiKeyRequirement())
                        .Build();
                });
        #endif

        services.AddScoped<IAuthorizationHandler, ApiKeyAuthorizationHandler>();
        services.RegisterApikeyServices(configuration, configureOptions, sectionName);
        return services;
    }

    public static IServiceCollection RegisterApiKeyPolicyAuthentication(
    this IServiceCollection services,
    Action<ApiKeyAuthenticationOptions> options)
    {
        return services.RegisterApiKeyPolicyAuthentication<ApiKeyValidator>(options);
    }

    public static IServiceCollection RegisterApiKeyPolicyAuthentication<TValidator>(this IServiceCollection services, 
        Action<ApiKeyAuthenticationOptions> options,
        string? defaultAuthenticateScheme = null,
        string? defaultChallengeScheme = null) where TValidator : class, IApiKeyValidator
    {
        services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = defaultAuthenticateScheme ?? ApiKeyAuthenticationOptions.Scheme;
                    options.DefaultChallengeScheme = defaultChallengeScheme ?? ApiKeyAuthenticationOptions.Scheme;
                })
                .AddApiKey<TValidator>(ApiKeyAuthenticationOptions.Scheme, options);

        return services;
    }

#if NET7_0_OR_GREATER
    public static IServiceCollection RegisterApiKeyEndpointFilter(
        this IServiceCollection services,
        IConfiguration? configuration = null,
        Action<ApiSettings>? configureOptions = null,
        string sectionName = Constants.ApiKeyName,
        bool useFactory = false)
    {
        if (!useFactory)
        {
            services.AddSingleton<ApiKeyEndpointFilter>();
        }

        services.RegisterApikeyServices(configuration, configureOptions, sectionName);

        return services;
    }
#endif

    public static IServiceCollection RegisterApiKeyMiddleware(
        this IServiceCollection services,
        IConfiguration? configuration = null,
        Action<ApiSettings>? configureOptions = null,
        string sectionName = Constants.ApiKeyName)
    {
        services.RegisterApikeyServices(configuration, configureOptions, sectionName);
        services.AddTransient<ApiKeyMiddleware>();

        return services;
    }

    public static IServiceCollection RegisterApikeyServices(
        this IServiceCollection services,
        IConfiguration? configuration = null,
        Action<ApiSettings>? configureOptions = null,
        string sectionName = Constants.ApiKeyName)
    {
        // Registra as opções conforme configuração ou ação
        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }
        else if (configuration != null)
        {
            services.Configure<ApiSettings>(configuration.GetSection(sectionName));
        }
        else
        {
            services.Configure<ApiSettings>(opts => { });
        }

        services.TryAddTransient<IApiKeyValidator, ApiKeyValidator>();
        return services;
    }
}
