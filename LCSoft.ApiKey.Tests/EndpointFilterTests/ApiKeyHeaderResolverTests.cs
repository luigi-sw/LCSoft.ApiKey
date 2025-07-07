#if NET7_0_OR_GREATER
using LCSoft.ApiKey.EndpointFilter;
using LCSoft.ApiKey.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace LCSoft.ApiKey.Tests.EndpointFilterTests;

public class ApiKeyHeaderResolverTests
{
    [Fact]
    public void Resolve_WithOverrideHeaderName_ReturnsOverride()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var options = Substitute.For<IOptions<ApiSettings>>();

        // Act
        var result = ApiKeyHeaderResolver.Resolve(context, options, "X-Override");

        // Assert
        Assert.Equal("X-Override", result);
    }

    [Fact]
    public void Resolve_WithEndpointMetadata_ReturnsHeaderFromMetadata()
    {
        // Arrange
        var context = new DefaultHttpContext();

        var metadata = new ApiKeyHeaderMetadata("X-Metadata");
        var endpoint = new Endpoint(
            requestDelegate: _ => Task.CompletedTask,
            metadata: new EndpointMetadataCollection(metadata),
            displayName: "Test"
        );

        context.SetEndpoint(endpoint);

        var options = Substitute.For<IOptions<ApiSettings>>();

        // Act
        var result = ApiKeyHeaderResolver.Resolve(context, options, null);

        // Assert
        Assert.Equal("X-Metadata", result);
    }

    [Fact]
    public void Resolve_WithOptions_ReturnsHeaderFromOptions()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var options = Options.Create(new ApiSettings { HeaderName = "X-Option" });

        // Act
        var result = ApiKeyHeaderResolver.Resolve(context, options, null);

        // Assert
        Assert.Equal("X-Option", result);
    }

    [Fact]
    public void Resolve_WithNoValues_ReturnsDefault()
    {
        // Arrange
        var context = new DefaultHttpContext();
        var options = Substitute.For<IOptions<ApiSettings>>();
        options.Value.Returns(new ApiSettings { HeaderName = null });

        // Act
        var result = ApiKeyHeaderResolver.Resolve(context, options, null);

        // Assert
        Assert.Equal(Constants.ApiKeyHeaderName, result);
    }

    [Fact]
    public void Resolve_PrioritizesOverrideOverAll()
    {
        // Arrange
        var context = new DefaultHttpContext();

        var metadata = new ApiKeyHeaderMetadata("X-Metadata");
        var endpoint = new Endpoint(
            requestDelegate: _ => Task.CompletedTask,
            metadata: new EndpointMetadataCollection(metadata),
            displayName: "Test"
        );
        context.SetEndpoint(endpoint);

        var options = Options.Create(new ApiSettings { HeaderName = "X-Option" });

        // Act
        var result = ApiKeyHeaderResolver.Resolve(context, options, "X-Override");

        // Assert
        Assert.Equal("X-Override", result);
    }

    [Fact]
    public void Resolve_WhenEndpointIsNull_DoesNotThrowAndReturnsFromOptionsOrDefault()
    {
        // Arrange
        var context = new DefaultHttpContext();

        // Não define endpoint no contexto, logo GetEndpoint() retorna null

        var options = Options.Create(new ApiSettings { HeaderName = "X-Option" });

        // Act
        var result = ApiKeyHeaderResolver.Resolve(context, options, null);

        // Assert
        Assert.Equal("X-Option", result);
    }

    [Fact]
    public void Resolve_WhenOptionsIsNull_ReturnsDefault()
    {
        // Arrange
        var context = new DefaultHttpContext();

        // Act
        var result = ApiKeyHeaderResolver.Resolve(context, null, null);

        // Assert
        Assert.Equal(Constants.ApiKeyHeaderName, result);
    }

    [Fact]
    public void Resolve_EndpointWithoutApiKeyHeaderMetadata_ReturnsOptionsHeader()
    {
        // Arrange
        var context = new DefaultHttpContext();

        // Endpoint com metadata que NÃO contém ApiKeyHeaderMetadata
        var endpoint = new Endpoint(
            _ => Task.CompletedTask,
            new EndpointMetadataCollection(new object[] { new object() }),
            "Test"
        );
        context.SetEndpoint(endpoint);

        var options = Options.Create(new ApiSettings { HeaderName = "X-Option" });

        // Act
        var result = ApiKeyHeaderResolver.Resolve(context, options, null);

        // Assert
        Assert.Equal("X-Option", result);
    }

    [Fact]
    public void Resolve_OptionsValueIsNull_ReturnsDefault()
    {
        // Arrange
        var context = new DefaultHttpContext();

        var options = Substitute.For<IOptions<ApiSettings>>();
        options.Value.Returns((ApiSettings)null!);  // simula Value null

        // Act
        var result = ApiKeyHeaderResolver.Resolve(context, options, null);

        // Assert
        Assert.Equal(Constants.ApiKeyHeaderName, result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Resolve_OptionsHeaderNameNullOrWhiteSpace_ReturnsDefault(string? headerName)
    {
        // Arrange
        var context = new DefaultHttpContext();

        var options = Options.Create(new ApiSettings { HeaderName = headerName });

        // Act
        var result = ApiKeyHeaderResolver.Resolve(context, options, null);

        // Assert
        Assert.Equal(Constants.ApiKeyHeaderName, result);
    }
}
#endif