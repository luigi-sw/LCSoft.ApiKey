#if NET7_0_OR_GREATER
using LCSoft.ApiKey.EndpointFilter;
using LCSoft.ApiKey.Models;
using LCSoft.ApiKey.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using NSubstitute;
using System.Net.Http.Headers;

namespace LCSoft.ApiKey.Tests.EndpointFilterTests;

public class ApiKeyEndpointFilterFactoryTests
{
    [Fact]
    public async Task InvokeAsync_NoApiKeyHeaderOrAuthorization_ReturnsMissing()
    {
        // Arrange
        var validator = Substitute.For<IApiKeyValidator>();
        var options = Options.Create(new ApiSettings { HeaderName = "X-Api-Key" });

        var services = Substitute.For<IServiceProvider>();
        services.GetService(typeof(IApiKeyValidator)).Returns(validator);
        services.GetService(typeof(IOptions<ApiSettings>)).Returns(options);

        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = services;

        var invocationContext = Substitute.For<EndpointFilterInvocationContext>();
        invocationContext.HttpContext.Returns(httpContext);

        var next = Substitute.For<EndpointFilterDelegate>();

        var filter = ApiKeyEndpointFilterFactory.CreateFactory()(null!, next);

        // Act
        var result = await filter(invocationContext);

        // Assert
        var unauthorized = Assert.IsType<UnauthorizedHttpObjectResult>(result);
        await next.DidNotReceive()(invocationContext);
    }

    [Fact]
    public async Task InvokeAsync_InvalidApiKey_ReturnsUnauthorized()
    {
        // Arrange
        var validator = Substitute.For<IApiKeyValidator>();
        validator.IsValid("bad-key").Returns(false);

        var options = Options.Create(new ApiSettings { HeaderName = "X-Api-Key" });

        var services = Substitute.For<IServiceProvider>();
        services.GetService(typeof(IApiKeyValidator)).Returns(validator);
        services.GetService(typeof(IOptions<ApiSettings>)).Returns(options);

        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = services;
        httpContext.Request.Headers["X-Api-Key"] = "bad-key";

        var invocationContext = Substitute.For<EndpointFilterInvocationContext>();
        invocationContext.HttpContext.Returns(httpContext);

        var next = Substitute.For<EndpointFilterDelegate>();

        var filter = ApiKeyEndpointFilterFactory.CreateFactory()(null!, next);

        // Act
        var result = await filter(invocationContext);

        // Assert
        var unauthorized = Assert.IsType<UnauthorizedHttpObjectResult>(result);
        await next.DidNotReceive()(invocationContext);
    }

    [Fact]
    public async Task InvokeAsync_ValidApiKey_FromAuthorizationHeader_CallsNext()
    {
        // Arrange
        var validator = Substitute.For<IApiKeyValidator>();
        validator.IsValid("valid-key").Returns(true);

        var options = Options.Create(new ApiSettings { HeaderName = "X-Api-Key" });

        var services = Substitute.For<IServiceProvider>();
        services.GetService(typeof(IApiKeyValidator)).Returns(validator);
        services.GetService(typeof(IOptions<ApiSettings>)).Returns(options);

        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = services;
        httpContext.Request.Headers[HeaderNames.Authorization] =
            new AuthenticationHeaderValue("ApiKey", "valid-key").ToString();

        var invocationContext = Substitute.For<EndpointFilterInvocationContext>();
        invocationContext.HttpContext.Returns(httpContext);

        var next = Substitute.For<EndpointFilterDelegate>();

        var expectedResult = "ok";
        next(invocationContext).Returns(new ValueTask<object?>(expectedResult)); // ✅ Use ValueTask

        var filter = ApiKeyEndpointFilterFactory.CreateFactory()(null!, next);

        // Act
        var result = await filter(invocationContext);

        // Assert
        Assert.Equal(expectedResult, result);
        await next.Received(1)(invocationContext);
    }

    [Fact]
    public async Task InvokeAsync_MalformedAuthorizationHeader_ReturnsMissing()
    {
        // Arrange
        var validator = Substitute.For<IApiKeyValidator>();

        var options = Options.Create(new ApiSettings { HeaderName = "X-Api-Key" });

        var services = Substitute.For<IServiceProvider>();
        services.GetService(typeof(IApiKeyValidator)).Returns(validator);
        services.GetService(typeof(IOptions<ApiSettings>)).Returns(options);

        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = services;

        httpContext.Request.Headers[HeaderNames.Authorization] = "";

        var invocationContext = Substitute.For<EndpointFilterInvocationContext>();
        invocationContext.HttpContext.Returns(httpContext);

        var next = Substitute.For<EndpointFilterDelegate>();

        var filter = ApiKeyEndpointFilterFactory.CreateFactory()(null!, next);

        // Act
        var result = await filter(invocationContext);

        // Assert
        var unauthorized = Assert.IsType<UnauthorizedHttpObjectResult>(result);
        await next.DidNotReceive()(invocationContext);
    }
}
#endif