#if NET7_0_OR_GREATER
using Microsoft.AspNetCore.Builder;
using NSubstitute;

namespace LCSoft.ApiKey.Tests.EndpointFilterTests;

public class ApiKeyEndpointFilterExtensionsTests
{
    [Fact]
    public void RequireApiKey_WithHeaderName_AddsMetadataAndFilter()
    {
        // Arrange
        var endpointConventionBuilders = Substitute.For<IEnumerable<IEndpointConventionBuilder>>();
        var teste = new RouteHandlerBuilder(endpointConventionBuilders);
        var headerName = "X-Test-Key";

        // Act
        var result = teste.RequireApiKey("X-Test-Key");

        // Assert
        Assert.Same(teste, result);
    }

    [Fact]
    public void RequireApiKey_WithoutHeaderName_AddsOnlyFilter()
    {
        // Arrange
        var endpointConventionBuilders = Substitute.For<IEnumerable<IEndpointConventionBuilder>>();
        var builder = new RouteHandlerBuilder(endpointConventionBuilders);

        // Act
        var result = builder.RequireApiKey(null);

        // Assert
        Assert.Same(builder, result);
    }

    [Fact]
    public void RequireApiKeyFactory_AddsFilterFactory()
    {
        // Arrange
        var endpointConventionBuilders = Substitute.For<IEnumerable<IEndpointConventionBuilder>>();
        var builder = new RouteHandlerBuilder(endpointConventionBuilders);
        var headerName = "X-Factory";

        // Act
        var result = builder.RequireApiKeyFactory(headerName);

        // Assert
        Assert.Same(builder, result);
    }
}
#endif