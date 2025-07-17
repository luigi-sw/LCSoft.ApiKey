using LCSoft.ApiKey.Attribute;
#if NET7_0_OR_GREATER
using LCSoft.ApiKey.EndpointFilter;
#endif
using LCSoft.ApiKey.Extensions;
using LCSoft.ApiKey.Middleware;
using LCSoft.ApiKey.Models;
using LCSoft.ApiKey.Policy.Auhtorization;
using LCSoft.ApiKey.Validation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LCSoft.ApiKey.Tests.Extensions;

public class ApiKeyExtensionsTests
{
    [Fact]
    public void RegisterApiKeyFilterAuthorization_WithConfiguration_AddsServices()
    {
        var services = new ServiceCollection();
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ApiKey:HeaderName"] = "X-Api-Key"
            })
        .Build();

        services.AddSingleton<Microsoft.Extensions.Configuration.IConfiguration>(config);
        services.AddSingleton(typeof(ILogger<>), typeof(Microsoft.Extensions.Logging.Abstractions.NullLogger<>));

        var result = services.RegisterApiKeyFilterAuthorization(config, "ApiKey");

        var provider = result.BuildServiceProvider();
        var filter = provider.GetService<ApiKeyAuthorizationFilter>();
        var options = provider.GetService<IOptions<ApiSettings>>();

        Assert.NotNull(filter);
        Assert.NotNull(options);
    }

    [Fact]
    public void RegisterApiKeyMiddleware_ResolvesMiddlewareWithoutError()
    {
        var inMemorySettings = new Dictionary<string, string?>
        {
            ["ApiKey:HeaderName"] = "X-Api-Key"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var services = new ServiceCollection();

        services.AddSingleton<Microsoft.Extensions.Configuration.IConfiguration>(configuration);
        services.AddSingleton(typeof(ILogger<>), typeof(Microsoft.Extensions.Logging.Abstractions.NullLogger<>));

        services.AddSingleton<RequestDelegate>(_ => context => Task.CompletedTask);

        services.RegisterApiKeyMiddleware(configuration);

        var provider = services.BuildServiceProvider();

        var middleware = ActivatorUtilities.CreateInstance<ApiKeyMiddleware>(provider);

        Assert.NotNull(middleware);
    }

    [Fact]
    public void RegisterApikeyServices_WhenConfigureOptionsIsProvided_ConfiguresOptions()
    {
        var services = new ServiceCollection();

        services.RegisterApikeyServices(
            configuration: null,
            configureOptions: opts => opts.HeaderName = "X-Test-Header",
            sectionName: "ApiKey");

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<ApiSettings>>();

        Assert.Equal("X-Test-Header", options.Value.HeaderName);
    }

    [Fact]
    public void RegisterApikeyServices_WhenNoConfigurationOrOptions_ConfiguresEmptyDefaults()
    {
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ApiKey:HeaderName"] = "X-Api-Key"
            })
        .Build();
        var services = new ServiceCollection();
        services.AddSingleton<Microsoft.Extensions.Configuration.IConfiguration>(config);
        services.RegisterApikeyServices();

        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<ApiSettings>>();

        Assert.NotNull(options.Value);
        Assert.NotNull(options.Value.HeaderName);
    }

    [Fact]
    public void RegisterApiKeyFilterAuthorization_WithConfigureOptions_RegistersFilterAndOptions()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?>
        {
            ["ApiKey:HeaderName"] = "X-Api-Key"
        };
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
        services.AddSingleton<Microsoft.Extensions.Configuration.IConfiguration>(configuration);
        services.AddSingleton(typeof(ILogger<>), typeof(Microsoft.Extensions.Logging.Abstractions.NullLogger<>));

        // Act
        services.RegisterApiKeyFilterAuthorization(options =>
        {
            options.HeaderName = "Test-Header";
        });

        var provider = services.BuildServiceProvider();

        // Assert
        var filter = provider.GetService<ApiKeyAuthorizationFilter>();
        var apiKeyOptions = provider.GetRequiredService<IOptions<ApiSettings>>();

        Assert.NotNull(filter);
        Assert.Equal("Test-Header", apiKeyOptions.Value.HeaderName);
    }

    [Fact]
    public void RegisterApiKeyCustomAuthorization_WithConfigureOptions_AddsCustomAuthorization()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.RegisterApiKeyCustomAuthorization(
            configuration: null,
            configureOptions: options => options.HeaderName = "X-Test",
            applyGlobally: true
        );

        var provider = services.BuildServiceProvider();

        // Assert
        var customAuth = provider.GetService<CustomAuthorization>();
        var apiKeyOptions = provider.GetRequiredService<IOptions<ApiSettings>>();

        Assert.NotNull(customAuth);
        Assert.Equal("X-Test", apiKeyOptions.Value.HeaderName);
    }

    [Fact]
    public void RegisterApiKeyCustomAuthorization_WithApplyGlobally_AddsFilterGlobally()
    {
        // Arrange
        var services = new ServiceCollection();

        // Ativa filtro global
        services.RegisterApiKeyCustomAuthorization(
            configuration: null,
            configureOptions: options => options.HeaderName = "X-Global",
            applyGlobally: true
        );

        var provider = services.BuildServiceProvider();

        // Força a configuração de MvcOptions
        var options = provider.GetRequiredService<IOptions<MvcOptions>>();

        // Act
        var filterExists = options.Value.Filters
            .OfType<ServiceFilterAttribute>()
            .Any(f => f.ServiceType == typeof(CustomAuthorization));

        // Assert
        Assert.True(filterExists);
    }

    [Fact]
    public void RegisterApiKeyCustomAuthorization_WithoutApplyGlobally_AddsFilterGlobally()
    {
        // Arrange
        var services = new ServiceCollection();

        // Ativa filtro global
        services.RegisterApiKeyCustomAuthorization(
            configuration: null,
            configureOptions: options => options.HeaderName = "X-Global",
            applyGlobally: false
        );

        var provider = services.BuildServiceProvider();

        // Força a configuração de MvcOptions
        var options = provider.GetRequiredService<IOptions<MvcOptions>>();

        // Act
        var filterExists = options.Value.Filters
            .OfType<ServiceFilterAttribute>()
            .Any(f => f.ServiceType == typeof(CustomAuthorization));

        // Assert
        Assert.False(filterExists);
    }

    [Fact]
    public void RegisterApiKeyPolicyAuthentication_WithOptions_AddsAuthenticationAndValidator()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?>
        {
            ["ApiKey:HeaderName"] = "X-Api-Key"
        };
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
        services.AddSingleton<Microsoft.Extensions.Configuration.IConfiguration>(configuration);
        services.AddSingleton(typeof(ILogger<>), typeof(Microsoft.Extensions.Logging.Abstractions.NullLogger<>));
        services.AddScoped<IApiKeyValidationStrategyFactory, ApiKeyValidationStrategyFactory>();
        services.AddScoped<IApiKeyValidationStrategy, DefaultApiKeyStrategy>();


        // Act
        services.RegisterApiKeyPolicyAuthentication(options =>
        {
            options.HeaderName = "X-Api-Key";
        });

        // Força pipeline de autenticação
        var provider = services.BuildServiceProvider();
        var authOptions = provider.GetRequiredService<IOptions<AuthenticationOptions>>().Value;
        // Assert
        var validator = provider.GetService<IApiKeyValidator>();
        Assert.NotNull(validator);
        Assert.IsType<ApiKeyValidator>(validator);
    }

#if NET7_0_OR_GREATER
    [Fact]
    public void RegisterApiKeyEndpointFilter_WithUseFactoryFalse_AddsSingletonFilter()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?>
        {
            ["ApiKey:HeaderName"] = "X-Api-Key"
        };
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
        services.AddSingleton<Microsoft.Extensions.Configuration.IConfiguration>(configuration);
        services.AddSingleton(typeof(ILogger<>), typeof(Microsoft.Extensions.Logging.Abstractions.NullLogger<>));

        // Act
        services.RegisterApiKeyEndpointFilter(
            configuration: null,
            configureOptions: opts => opts.HeaderName = "X-Test",
            sectionName: "ApiKey",
            useFactory: false);

        var provider = services.BuildServiceProvider();

        // Assert
        var filter = provider.GetService<ApiKeyEndpointFilter>();
        var options = provider.GetService<IOptions<ApiSettings>>();

        Assert.NotNull(filter);
        Assert.NotNull(options);
        Assert.Equal("X-Test", options.Value.HeaderName);
    }

    [Fact]
    public void RegisterApiKeyEndpointFilter_WithUseFactoryTrue_DoesNotAddSingletonFilter()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.RegisterApiKeyEndpointFilter(
            configuration: null,
            configureOptions: opts => opts.HeaderName = "X-Test",
            sectionName: "ApiKey",
            useFactory: true);

        var provider = services.BuildServiceProvider();

        // Assert
        var filter = provider.GetService<ApiKeyEndpointFilter>();

        Assert.Null(filter); // porque não adicionou singleton
    }
#endif

    [Fact]
    public void RegisterApiKeyPolicyAuthorization_RegistersCorrectServices_NET6()
    {
#if !NET7_0_OR_GREATER
        // Arrange
        var inMemorySettings = new Dictionary<string, string?>
        {
            ["ApiKey:HeaderName"] = "X-Api-Key"
        };
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
        services.AddSingleton<Microsoft.Extensions.Configuration.IConfiguration>(configuration);
        services.AddScoped<IApiKeyValidationStrategyFactory, ApiKeyValidationStrategyFactory>();
        services.AddScoped<IApiKeyValidationStrategy, DefaultApiKeyStrategy>();
        services.AddLogging();
        // Act
        services.RegisterApiKeyPolicyAuthorization(
            configuration: null,
            configureOptions: opts => opts.HeaderName = "X-Api-Key");

        var provider = services.BuildServiceProvider();

        // Assert
        var authHandler = provider.GetService<IAuthorizationHandler>();
        Assert.NotNull(authHandler);
        Assert.IsType<ApiKeyAuthorizationHandler>(authHandler);

        var options = provider.GetRequiredService<IOptions<ApiSettings>>();
        Assert.Equal("X-Api-Key", options.Value.HeaderName);
#endif
    }

    [Fact]
    public void RegisterApiKeyPolicyAuthorization_RegistersCorrectServices_NET7_OR_GREATER()
    {
#if NET7_0_OR_GREATER
        // Arrange
        var inMemorySettings = new Dictionary<string, string?>
        {
            ["ApiKey:HeaderName"] = "X-Api-Key"
        };
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
        services.AddSingleton<Microsoft.Extensions.Configuration.IConfiguration>(configuration);
services.AddSingleton(typeof(ILogger<>), typeof(Microsoft.Extensions.Logging.Abstractions.NullLogger<>));

        // Act
        services.RegisterApiKeyPolicyAuthorization(
            configuration: null,
            configureOptions: opts => opts.HeaderName = "X-Api-Key");

        var provider = services.BuildServiceProvider();

        // Assert
        var authHandler = provider.GetService<IAuthorizationHandler>();
        Assert.NotNull(authHandler);
        Assert.IsType<ApiKeyAuthorizationHandler>(authHandler);

        var options = provider.GetRequiredService<IOptions<ApiSettings>>();
        Assert.Equal("X-Api-Key", options.Value.HeaderName);
#endif
    }


    [Fact]
    public void RegisterApiKeyPolicyAuthorization_ConfiguresAuthenticationAndAuthorization_ForNet7OrGreater()
    {
#if NET7_0_OR_GREATER
        // Arrange
        var inMemorySettings = new Dictionary<string, string?>
        {
            ["ApiKey:HeaderName"] = "X-Api-Key"
        };
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
        services.AddSingleton<Microsoft.Extensions.Configuration.IConfiguration>(configuration);
        services.AddSingleton(typeof(ILogger<>), typeof(Microsoft.Extensions.Logging.Abstractions.NullLogger<>));

        // Act
        var result = ApiKeyExtensions.RegisterApiKeyPolicyAuthorization(services);

        // Assert
        var provider = result.BuildServiceProvider();

        // Valida se o handler foi registrado
        var handler = provider.GetService<IAuthorizationHandler>();
        Assert.NotNull(handler);
        Assert.IsType<ApiKeyAuthorizationHandler>(handler);

        // Valida se o serviço de autenticação foi adicionado
        var authOptions = provider.GetRequiredService<IOptions<AuthenticationOptions>>().Value;
        Assert.Equal(JwtBearerDefaults.AuthenticationScheme, authOptions.DefaultAuthenticateScheme);
#endif
    }

#if NET6_0
    [Fact]
    public void RegisterApiKeyPolicyAuthorization_RegistersBearerPolicy_ForNet6()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?>
        {
            ["ApiKey:HeaderName"] = "X-Api-Key"
        };
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();
        services.AddSingleton<Microsoft.Extensions.Configuration.IConfiguration>(configuration);
        services.AddScoped<IApiKeyValidationStrategyFactory, ApiKeyValidationStrategyFactory>();
        services.AddScoped<IApiKeyValidationStrategy, DefaultApiKeyStrategy>();
        services.AddLogging();

        // Act
        services.RegisterApiKeyPolicyAuthorization();

        var provider = services.BuildServiceProvider();

        // Assert Auth
        var authOptions = provider.GetRequiredService<IOptions<AuthenticationOptions>>().Value;
        Assert.Equal("Bearer", authOptions.DefaultAuthenticateScheme);
        Assert.Equal("Bearer", authOptions.DefaultChallengeScheme);

        // Assert Authorization Policy
        var authzOptions = provider.GetRequiredService<IOptions<AuthorizationOptions>>().Value;
        var policy = authzOptions.DefaultPolicy;

        Assert.NotNull(policy);
        Assert.Contains(policy.Requirements, r => r is ApiKeyRequirement);

        // Assert Authorization Handler
        var handler = provider.GetService<IAuthorizationHandler>();
        Assert.NotNull(handler);
        Assert.IsType<ApiKeyAuthorizationHandler>(handler);
    }

#endif
}
