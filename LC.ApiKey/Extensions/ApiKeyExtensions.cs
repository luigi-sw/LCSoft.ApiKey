using LC.ApiKey.Attribute;
using LC.ApiKey.EndpointFilter;
using LC.ApiKey.Models;
using LC.ApiKey.Policy.Auhtorization;
using LC.ApiKey.Policy.Authentication;
using LC.ApiKey.Services;
using LC.ApiKey.Validation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LC.ApiKey.Extensions;

public static class ApiKeyExtensions
{

    public static IServiceCollection RegisterApiKeyFilterAuthorization(
        this IServiceCollection services,
        Action<ApiSettings> configureOptions,
        string sectionName = Constants.ApiKeyName) =>
            RegisterApiKeyFilterAuthorization(services, null, configureOptions, sectionName);

    public static IServiceCollection RegisterApiKeyFilterAuthorization(
        this IServiceCollection services,
        IConfiguration configuration,
        string sectionName = Constants.ApiKeyName) =>
            RegisterApiKeyFilterAuthorization(services, configuration, null, sectionName);

    public static IServiceCollection RegisterApiKeyFilterAuthorization(this IServiceCollection services,
        IConfiguration? configuration = null,
        Action<ApiSettings>? configureOptions = null,
        string sectionName = Constants.ApiKeyName)
    {
        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }
        else if (configuration != null)
        {
            services.Configure<ApiSettings>(configuration.GetSection(sectionName));
        }

        services.AddScoped<ApiKeyAuthorizationFilter>();
        services.RegisterApikeyServices();
        return services;
    }

    public static IServiceCollection RegisterApiKeyCustomAuthorization(
       this IServiceCollection services,
       IConfiguration? configuration = null,
       Action<ApiSettings>? configureOptions = null,
       string sectionName = "ApiKey",
       bool applyGlobally = false)
    {
        services.RegisterApikeyServices();

        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }
        else if (configuration != null)
        {
            services.Configure<ApiSettings>(configuration.GetSection(sectionName));
        }

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

    public static IServiceCollection RegisterApiKeyPolicyAuthorization(this IServiceCollection services)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer();
        //services.AddAuthentication() // No default scheme needed
        //.AddScheme<AuthenticationSchemeOptions, NullAuthHandler>("NullAuth", _ => { });

        services.AddAuthorizationBuilder().AddDefaultPolicy("", policy =>
        {
            policy.AddAuthenticationSchemes(new[] { JwtBearerDefaults.AuthenticationScheme });
            //policy.AddAuthenticationSchemes("NullAuth");
            policy.Requirements.Add(new ApiKeyRequirement());
        });

        services.AddScoped<IAuthorizationHandler, ApiKeyAuthorizationHandler>();
        services.RegisterApikeyServices();
        return services;
    }

    public static IServiceCollection RegisterApiKeyPolicyAuthorization(this IServiceCollection services, List<string> validApiKeys)
    {
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer();
        services.AddTransient<IAuthorizationHandler, ApiKeyAuthorizationHandler>();
        services.AddAuthorizationBuilder()
                .AddDefaultPolicy("", policyBuilder =>
                {
                    policyBuilder.AddAuthenticationSchemes(new[] { JwtBearerDefaults.AuthenticationScheme });
                    policyBuilder.AddRequirements(new ApiKeyRequirement(validApiKeys));
                });
        services.RegisterApikeyServices();
        return services;
    }
    
    public static IServiceCollection RegisterApiKeyPolicyAuthentication(this IServiceCollection services, Action<ApiKeyAuthenticationOptions> options)
    {
        //services.AddAuthentication(Constants.AuthenticationScheme)
        //        .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(Constants.AuthenticationScheme, null);

        //Register the Authentication service Handler that will be consumed by the handler.
        //services.AddSingleton<IApiKeyAuthenticationService, ApiKeyAuthenticationService>();

        //Or, in a more elegant way, using the extensions:

        services.AddAuthentication(Constants.Scheme)
                .AddApiKey<ApiKeyAuthenticationService>(ApiKeyAuthenticationOptions.Scheme, options);

        services.RegisterApikeyServices();
        return services;

        //services.AddAuthorization();
        //services.AddAuthentication(ApiKeyAuthenticationOptions.Scheme)
        //    .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
        //        ApiKeyAuthenticationOptions.Scheme, options =>
        //        {
        //            options.HeaderName = "X-API-KEY";
        //        });
    }
    
    public static IServiceCollection RegisterMVCApikey(this IServiceCollection services)
    {
        //usage: [ServiceFilter(typeof(ApiKeyAuthorizationFilter))]
        services.RegisterApikeyServices();
        return services;

        /*
         // Add API Key Swagger support
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("ApiKey", new OpenApiSecurityScheme
    {
        Description = "The API Key to access the API",
        Type = SecuritySchemeType.ApiKey,
        Name = AuthConstants.ApiKeyHeaderName,
        In = ParameterLocation.Header,
        Scheme = "ApiKeyScheme"
    });

    var scheme = new OpenApiSecurityScheme
    {
        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = "ApiKey"
        },
        In = ParameterLocation.Header
    };

    var requirement = new OpenApiSecurityRequirement
    {
        {scheme, new List<string>() }
    };

    c.AddSecurityRequirement(requirement);
});
         */
    }

    public static IServiceCollection RegisterApiKeyEndpointFilter(
        this IServiceCollection services,
        IConfiguration? configuration = null,
        Action<ApiSettings>? configureOptions = null,
        string sectionName = "ApiKey",
        bool useFactory = false)
    {
        services.RegisterApikeyServices();

        if (!useFactory)
        {
            services.AddSingleton<ApiKeyEndpointFilter>();
        }

        if (configureOptions != null)
        {
            services.Configure(configureOptions);
        }
        else if (configuration != null)
        {
            services.Configure<ApiSettings>(configuration.GetSection(sectionName));
        }

        return services;
    }

    public static IServiceCollection RegisterApikeyServices(this IServiceCollection services)
    {
        services.TryAddTransient<IApiKeyValidator, ApiKeyValidator>();
        return services;
    }
}
