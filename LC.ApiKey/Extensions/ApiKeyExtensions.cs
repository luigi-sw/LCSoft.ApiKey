using LC.ApiKey.Attribute;
using LC.ApiKey.Policy.Auhtorization;
using LC.ApiKey.Policy.Authentication;
using LC.ApiKey.Services;
using LC.ApiKey.Validation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LC.ApiKey.Extensions;

public static class ApiKeyExtensions
{
    public static IServiceCollection RegisterApiKeyFilterAuthorization(this IServiceCollection services)
    {
        //services.AddAuthentication();
        services.AddScoped<ApiKeyAuthorizationFilter>();
        services.RegisterApikeyServices();
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

    public static IServiceCollection RegisterApikeyServices(this IServiceCollection services)
    {
        services.TryAddTransient<IApiKeyValidator, ApiKeyValidator>();
        return services;
    }
    
    public static IServiceCollection RegisterMVCApikey(this IServiceCollection services)
    {
        // Controller with added filter for EVERY controller
        services.AddControllers(x => x.Filters.Add<ApiKeyAuthorizationFilter>());
        // Register the AuthKeyFilter for single controllers
        //services.AddScoped<ApiKeyAuthorizationFilter>(); 
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
}
