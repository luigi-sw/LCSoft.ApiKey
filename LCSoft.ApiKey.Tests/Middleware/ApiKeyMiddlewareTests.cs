using LCSoft.ApiKey.Middleware;
using LCSoft.ApiKey.Models;
using LCSoft.ApiKey.Validation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.Options;
using NSubstitute;
using System.Net;
using System.Reflection;

namespace LCSoft.ApiKey.Tests.Middleware;

public class ApiKeyMiddlewareTests
{
    private readonly RequestDelegate _next = Substitute.For<RequestDelegate>();
    private readonly IApiKeyValidator _validator = Substitute.For<IApiKeyValidator>();
    private readonly ApiSettings _settings = new ApiSettings { HeaderName = "X-Api-Key" };
    private readonly IOptions<ApiSettings> _options;

    public ApiKeyMiddlewareTests()
    {
        _options = Options.Create(_settings);
    }

    private static HttpContext CreateHttpContext(string? apiKey = null, string? authorizationHeader = null, Endpoint? endpoint = null)
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        if (apiKey != null)
            context.Request.Headers["X-Api-Key"] = apiKey;

        if (authorizationHeader != null)
            context.Request.Headers["Authorization"] = authorizationHeader;

        context.SetEndpoint(endpoint);

        return context;
    }

    [Fact]
    public async Task InvokeAsync_WithAllowAnonymousAttributeOnController_SkipsValidation()
    {
        var context = CreateHttpContext(endpoint: CreateEndpointWithControllerAllowAnonymous());

        var middleware = new ApiKeyMiddleware(_next, _validator, _options);

        await middleware.InvokeAsync(context);

        await _next.Received(1).Invoke(context);
    }

    [Fact]
    public async Task InvokeAsync_WithAllowAnonymousAttributeOnMinimalApi_SkipsValidation()
    {
        var endpoint = new Endpoint(
            context => Task.CompletedTask,
            new EndpointMetadataCollection(new AllowAnonymousAttribute()),
            "test");

        var context = CreateHttpContext(endpoint: endpoint);

        var middleware = new ApiKeyMiddleware(_next, _validator, _options);

        await middleware.InvokeAsync(context);

        await _next.Received(1).Invoke(context);
    }

    [Fact]
    public async Task InvokeAsync_WithValidApiKeyHeader_AllowsRequest()
    {
        _validator.IsValid("valid-key").Returns(true);
        var context = CreateHttpContext(apiKey: "valid-key");

        var middleware = new ApiKeyMiddleware(_next, _validator, _options);

        await middleware.InvokeAsync(context);

        await _next.Received(1).Invoke(context);
    }

    [Fact]
    public async Task InvokeAsync_WithInvalidApiKeyHeader_ReturnsUnauthorized()
    {
        _validator.IsValid("invalid-key").Returns(false);
        var context = CreateHttpContext(apiKey: "invalid-key");

        var middleware = new ApiKeyMiddleware(_next, _validator, _options);

        await middleware.InvokeAsync(context);

        Assert.Equal((int)HttpStatusCode.Unauthorized, context.Response.StatusCode);
        await _next.DidNotReceive().Invoke(context);
    }

    [Fact]
    public async Task InvokeAsync_WithoutApiKey_ReturnsUnauthorized()
    {
        var context = CreateHttpContext();

        var middleware = new ApiKeyMiddleware(_next, _validator, _options);

        await middleware.InvokeAsync(context);

        Assert.Equal((int)HttpStatusCode.Unauthorized, context.Response.StatusCode);
        await _next.DidNotReceive().Invoke(context);
    }

    [Fact]
    public async Task InvokeAsync_WithApiKeyInAuthorizationHeader_Valid()
    {
        var wasCalled = false;
        RequestDelegate next = context =>
        {
            wasCalled = true;
            return Task.CompletedTask;
        };
        _validator.IsValid("auth-key").Returns(true);
        var context = CreateHttpContext(authorizationHeader: "ApiKey auth-key");

        var middleware = new ApiKeyMiddleware(next, _validator, _options);
        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(wasCalled, "The next delegate was not called");
    }

    [Fact]
    public async Task InvokeAsync_WithAuthorizationHeaderInvalidFormat_ReturnsUnauthorized()
    {
        var context = CreateHttpContext(authorizationHeader: "Bearer something");

        var middleware = new ApiKeyMiddleware(_next, _validator, _options);

        await middleware.InvokeAsync(context);

        Assert.Equal((int)HttpStatusCode.Unauthorized, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_WithMalformedAuthorizationHeader_ReturnsUnauthorized()
    {
        // Arrange
        var wasCalled = false;

        RequestDelegate next = ctx =>
        {
            wasCalled = true;
            return Task.CompletedTask;
        };

        var validator = Substitute.For<IApiKeyValidator>();
        var options = Options.Create(new ApiSettings { HeaderName = "X-Api-Key" });

        var context = new DefaultHttpContext();
        context.Request.Headers["Authorization"] = "";

        var middleware = new ApiKeyMiddleware(next, validator, options);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.False(wasCalled, "The next delegate should not be called for malformed Authorization header.");
        Assert.Equal(401, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_WithMvcAllowAnonymousAttribute_SkipsValidation()
    {
        // Arrange

        var validator = Substitute.For<IApiKeyValidator>();
        var options = Options.Create(new ApiSettings());

        var context = new DefaultHttpContext();

        var methodInfo = typeof(DummyController).GetMethod(nameof(DummyController.ActionWithoutAllowAnonymous));
        var descriptor = new ControllerActionDescriptor
        {
            MethodInfo = methodInfo!,
            ControllerTypeInfo = typeof(DummyController).GetTypeInfo()
        };

        var metadata = new EndpointMetadataCollection(descriptor);
        context.SetEndpoint(new Endpoint(_ => Task.CompletedTask, metadata, "mvc"));

        var middleware = new ApiKeyMiddleware(_next, validator, options);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        await _next.DidNotReceive().Invoke(context);
    }

    [Fact]
    public async Task InvokeAsync_WithEmptyHeaderName_UsesDefaultHeaderConstant()
    {
        // Arrange
        var wasCalled = false;
        RequestDelegate next = ctx =>
        {
            wasCalled = true;
            return Task.CompletedTask;
        };

        var validator = Substitute.For<IApiKeyValidator>();
        validator.IsValid("xyz456").Returns(true);

        var options = Options.Create(new ApiSettings
        {
            HeaderName = ""
        });

        var context = new DefaultHttpContext();
        context.Request.Headers[Constants.ApiKeyHeaderName] = "xyz456";

        var middleware = new ApiKeyMiddleware(next, validator, options);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(wasCalled);
    }

    [Fact]
    public async Task InvokeAsync_WithMinimalApiAllowAnonymous_SkipsValidation()
    {
        // Arrange
        var validator = Substitute.For<IApiKeyValidator>();
        var options = Options.Create(new ApiSettings());

        var context = new DefaultHttpContext();

        var metadata = new EndpointMetadataCollection();
        context.SetEndpoint(new Endpoint(_ => Task.CompletedTask, metadata, "minimal"));

        var middleware = new ApiKeyMiddleware(_next, validator, options);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        await _next.DidNotReceive().Invoke(context);
    }

    private static Endpoint CreateEndpointWithControllerAllowAnonymous()
    {
        var method = typeof(DummyController).GetMethod(nameof(DummyController.AnonymousAction));
        var descriptor = new ControllerActionDescriptor
        {
            MethodInfo = method!,
            ControllerTypeInfo = typeof(DummyController).GetTypeInfo()
        };

        var metadata = new EndpointMetadataCollection(descriptor);
        return new Endpoint(context => Task.CompletedTask, metadata, "test");
    }

    public class DummyController
    {
        [AllowAnonymous]
        public void AnonymousAction() { }

        public void ActionWithoutAllowAnonymous() { }

    }
}
