using LCSoft.ApiKey.Extensions;
using LCSoft.ApiKey.Models;
using LCSoft.ApiKey.Policy.Authentication;
using LCSoft.ApiKey.Validation;
using LCSoft.Results;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace LCSoft.ApiKey.Tests.Extensions;

public class ApiKeyAuthenticationExtensionsTests
{
    private class FakeAuthenticationBuilder : AuthenticationBuilder
    {
        public IServiceCollection ServicesCollection { get; }

        public bool AddSchemeCalled { get; private set; } = false;
        public string? SchemeNamePassed { get; private set; }
        public Type? HandlerTypePassed { get; private set; }
        public Action<ApiKeyAuthenticationOptions>? OptionsActionPassed { get; private set; }

        public FakeAuthenticationBuilder(IServiceCollection services) : base(services)
        {
            ServicesCollection = services;
        }

        public override AuthenticationBuilder AddScheme<TOptions, THandler>(string authenticationScheme, Action<TOptions> configureOptions)
        {
            AddSchemeCalled = true;
            SchemeNamePassed = authenticationScheme;
            HandlerTypePassed = typeof(THandler);

            // Since TOptions is generic, we need to cast Action<TOptions> to Action<ApiKeyAuthenticationOptions>
            OptionsActionPassed = configureOptions as Action<ApiKeyAuthenticationOptions>;

            return this;
        }
    }

    [Fact]
    public void AddApiKey_NoParams_AddsServicesAndScheme()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new FakeAuthenticationBuilder(services);

        // Act
        var result = ApiKeyAuthenticationExtensions.AddApiKey<FakeApiKeyValidator>(builder);

        // Assert
        Assert.NotNull(result);
        Assert.True(builder.AddSchemeCalled);
        Assert.Equal(ApiKeyAuthenticationOptions.Scheme, builder.SchemeNamePassed);
        Assert.Equal(typeof(ApiKeyAuthenticationHandler), builder.HandlerTypePassed);

        // Verifica que os serviços foram adicionados
        Assert.Contains(services, sd => sd.ServiceType == typeof(IPostConfigureOptions<ApiKeyAuthenticationOptions>));
        Assert.Contains(services, sd => sd.ServiceType == typeof(IApiKeyValidator) && sd.Lifetime == ServiceLifetime.Transient);
    }

    [Fact]
    public void AddApiKey_WithScheme_AddsServicesAndSchemeWithScheme()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new FakeAuthenticationBuilder(services);
        string customScheme = "CustomScheme";

        // Act
        var result = ApiKeyAuthenticationExtensions.AddApiKey<FakeApiKeyValidator>(builder, customScheme);

        // Assert
        Assert.NotNull(result);
        Assert.True(builder.AddSchemeCalled);
        Assert.Equal(customScheme, builder.SchemeNamePassed);
    }

    [Fact]
    public void AddApiKey_WithOptions_AddsServicesAndSchemeWithOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new FakeAuthenticationBuilder(services);

        bool configureCalled = false;
        Action<ApiKeyAuthenticationOptions> configure = opts => configureCalled = true;

        // Act
        var result = ApiKeyAuthenticationExtensions.AddApiKey<FakeApiKeyValidator>(builder, configure);

        // Assert
        Assert.NotNull(result);
        Assert.True(builder.AddSchemeCalled);
        Assert.Equal(ApiKeyAuthenticationOptions.Scheme, builder.SchemeNamePassed);

        // Executa a action para garantir que ela funciona
        builder.OptionsActionPassed?.Invoke(new ApiKeyAuthenticationOptions());
        Assert.True(configureCalled);
    }

    [Fact]
    public void AddApiKey_WithSchemeAndOptions_AddsServicesAndSchemeWithSchemeAndOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new FakeAuthenticationBuilder(services);
        string customScheme = "CustomScheme";

        bool configureCalled = false;
        Action<ApiKeyAuthenticationOptions> configure = opts => configureCalled = true;

        // Act
        var result = ApiKeyAuthenticationExtensions.AddApiKey<FakeApiKeyValidator>(builder, customScheme, configure);

        // Assert
        Assert.NotNull(result);
        Assert.True(builder.AddSchemeCalled);
        Assert.Equal(customScheme, builder.SchemeNamePassed);

        builder.OptionsActionPassed?.Invoke(new ApiKeyAuthenticationOptions());
        Assert.True(configureCalled);
    }

    [Fact]
    public void AddApiKey_Parameterless_CallsOverloadWithDefaultScheme()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new FakeAuthenticationBuilder(services);

        // Act
        // Chama exatamente o método com a assinatura sem parâmetros extras
        var result = ApiKeyAuthenticationExtensions.AddApiKey<FakeApiKeyValidator>(builder);

        // Assert
        Assert.NotNull(result);
        Assert.True(builder.AddSchemeCalled);
        Assert.Equal(ApiKeyAuthenticationOptions.Scheme, builder.SchemeNamePassed);
    }

    [Fact]
    public void AddApiKey_WithSchemeOnly_CallsOverloadWithEmptyConfigure()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new FakeAuthenticationBuilder(services);
        string customScheme = "MyScheme";

        // Act
        // Chama exatamente o método com esquema customizado e sem configuração
        var result = ApiKeyAuthenticationExtensions.AddApiKey<FakeApiKeyValidator>(builder, customScheme);

        // Assert
        Assert.NotNull(result);
        Assert.True(builder.AddSchemeCalled);
        Assert.Equal(customScheme, builder.SchemeNamePassed);
    }
    
    [Fact]
    public void AddApiKey_WithoutParameters_ExecutesWrapperMethod()
    {
        // Arrange
        var services = new ServiceCollection();
        var builder = new FakeAuthenticationBuilder(services);

        // Act
        var result = ApiKeyAuthenticationExtensions.AddApiKey<FakeApiKeyValidator>(builder, Constants.Scheme);

        // Assert
        Assert.NotNull(result);
        Assert.True(builder.AddSchemeCalled);
        Assert.Equal(ApiKeyAuthenticationOptions.Scheme, builder.SchemeNamePassed);
    }

    [Fact]
    public void AddApiKeyWithDefaults_CallsAddApiKeyWithEmptyConfigure()
    {
        var services = new ServiceCollection();
        var builder = new FakeAuthenticationBuilder(services);
        string scheme = "MyScheme";

        var result = ApiKeyAuthenticationExtensions.AddApiKeyWithDefaults<FakeApiKeyValidator>(builder, scheme);

        Assert.NotNull(result);
        Assert.True(builder.AddSchemeCalled);
        Assert.Equal(scheme, builder.SchemeNamePassed);
    }

    // Fake IApiKeyValidator só para testes
    private class FakeApiKeyValidator : IApiKeyValidator { public bool IsValid(string apiKey) => true;

        public Results<ApiKeyInfo> ValidateAndGetInfo(string apiKey)
        {
            throw new NotImplementedException();
        }
    }

}
