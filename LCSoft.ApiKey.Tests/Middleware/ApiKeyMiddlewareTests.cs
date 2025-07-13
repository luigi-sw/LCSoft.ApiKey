using LCSoft.ApiKey.Middleware;
using LCSoft.ApiKey.Models;
using LCSoft.ApiKey.Validation;
using LCSoft.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using System.ComponentModel.DataAnnotations;
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

    private static HttpContext CreateHttpContext(string? apiKey = null, 
        string? authorizationHeader = null, 
        Endpoint? endpoint = null,
        ServiceCollection? services = null)
    {
        if (services == null)
        {
            services = new ServiceCollection();
            services.AddSingleton(Substitute.For<IApiKeyValidator>());
            services.AddSingleton(Substitute.For<IApiKeyValidationStrategy>());
            services.AddSingleton(Substitute.For<IApiKeyValidationStrategyFactory>());
            services.AddSingleton(Options.Create(new ApiSettings { HeaderName = "X-Api-Key" }));
        }

        var serviceProvider = services.BuildServiceProvider();

        var context = new DefaultHttpContext()
        {
            RequestServices = serviceProvider
        };
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

        var middleware = new ApiKeyMiddleware(_next);

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

        var middleware = new ApiKeyMiddleware(_next);

        await middleware.InvokeAsync(context);

        await _next.Received(1).Invoke(context);
    }

    [Fact]
    public async Task InvokeAsync_WithValidApiKeyHeader_AllowsRequest()
    {
        IApiKeyValidator validator = Substitute.For<IApiKeyValidator>();
        IOptions<ApiSettings> options = Options.Create(new ApiSettings { HeaderName = "X-Api-Key" });
        var services = new ServiceCollection();
        services.AddSingleton(validator);
        services.AddSingleton(options);

        validator.IsValid("valid-key").Returns(Results<bool>.Success(true));
        var context = CreateHttpContext(apiKey: "valid-key", services: services);

        var middleware = new ApiKeyMiddleware(_next);

        await middleware.InvokeAsync(context);

        await _next.Received(1).Invoke(context);
    }

    [Fact]
    public async Task InvokeAsync_WithInvalidApiKeyHeader_ReturnsUnauthorized()
    {
        IApiKeyValidator validator = Substitute.For<IApiKeyValidator>();
        IOptions<ApiSettings> options = Options.Create(new ApiSettings { HeaderName = "X-Api-Key" });
        var services = new ServiceCollection();
        services.AddSingleton(validator);
        services.AddSingleton(options);

        validator.IsValid("invalid-key").Returns(Results<bool>.Failure(StandardErrorType.GenericFailure));
        var context = CreateHttpContext(apiKey: "invalid-key", services: services);

        var middleware = new ApiKeyMiddleware(_next);

        await middleware.InvokeAsync(context);

        Assert.Equal((int)HttpStatusCode.Unauthorized, context.Response.StatusCode);
        await _next.DidNotReceive().Invoke(context);
    }

    [Fact]
    public async Task InvokeAsync_WithoutApiKey_ReturnsUnauthorized()
    {
        var validator = Substitute.For<IApiKeyValidator>();
        IOptions<ApiSettings> options = Options.Create(new ApiSettings { HeaderName = "X-Api-Key" });
        var services = new ServiceCollection();
        services.AddSingleton(validator);
        services.AddSingleton(options);
        validator.IsValid(Arg.Any<string>()).Returns(Results<bool>.Failure(StandardErrorType.GenericFailure));
        var context = CreateHttpContext(services: services);

        var middleware = new ApiKeyMiddleware(_next);

        await middleware.InvokeAsync(context);

        Assert.Equal((int)HttpStatusCode.Unauthorized, context.Response.StatusCode);
        await _next.DidNotReceive().Invoke(context);
    }

    [Fact]
    public async Task InvokeAsync_WithApiKeyInAuthorizationHeader_Valid()
    {
        var validator = Substitute.For<IApiKeyValidator>();
        IOptions<ApiSettings> options = Options.Create(new ApiSettings { HeaderName = "X-Api-Key" });
        var services = new ServiceCollection();
        services.AddSingleton(validator);
        services.AddSingleton(options);
        validator.IsValid("auth-key").Returns(Results<bool>.Success(true));
        var wasCalled = false;
        RequestDelegate next = context =>
        {
            wasCalled = true;
            return Task.CompletedTask;
        };
        var context = CreateHttpContext(authorizationHeader: "ApiKey auth-key", services: services);

        var middleware = new ApiKeyMiddleware(next);
        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(wasCalled, "The next delegate was not called");
    }

    [Fact]
    public async Task InvokeAsync_WithAuthorizationHeaderInvalidFormat_ReturnsUnauthorized()
    {
        var validator = Substitute.For<IApiKeyValidator>();
        IOptions<ApiSettings> options = Options.Create(new ApiSettings { HeaderName = "X-Api-Key" });
        var services = new ServiceCollection();
        services.AddSingleton(validator);
        services.AddSingleton(options);
        validator.IsValid(Arg.Any<string>()).Returns(Results<bool>.Failure(StandardErrorType.GenericFailure));

        var context = CreateHttpContext(authorizationHeader: "Bearer something", services: services);

        var middleware = new ApiKeyMiddleware(_next);

        await middleware.InvokeAsync(context);

        Assert.Equal((int)HttpStatusCode.Unauthorized, context.Response.StatusCode);
    }

    [Fact]
    public async Task InvokeAsync_WithMalformedAuthorizationHeader_ReturnsUnauthorized()
    {
        // Arrange
        var services = new ServiceCollection();
        var wasCalled = false;

        RequestDelegate next = ctx =>
        {
            wasCalled = true;
            return Task.CompletedTask;
        };

        var validator = Substitute.For<IApiKeyValidator>();
        var options = Options.Create(new ApiSettings { HeaderName = "X-Api-Key" });
        services.AddSingleton(validator);
        services.AddSingleton(options);

        var context = new DefaultHttpContext() 
        {
            RequestServices = services.BuildServiceProvider()
        };
        context.Request.Headers["Authorization"] = "";

        var middleware = new ApiKeyMiddleware(next);

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
        var services = new ServiceCollection();
        var validator = Substitute.For<IApiKeyValidator>();
        var options = Options.Create(new ApiSettings());
        services.AddSingleton(validator);
        services.AddSingleton(options);

        var context = new DefaultHttpContext()
        {
            RequestServices = services.BuildServiceProvider()
        };

        var methodInfo = typeof(DummyController).GetMethod(nameof(DummyController.ActionWithoutAllowAnonymous));
        var descriptor = new ControllerActionDescriptor
        {
            MethodInfo = methodInfo!,
            ControllerTypeInfo = typeof(DummyController).GetTypeInfo()
        };

        var metadata = new EndpointMetadataCollection(descriptor);
        context.SetEndpoint(new Endpoint(_ => Task.CompletedTask, metadata, "mvc"));

        var middleware = new ApiKeyMiddleware(_next);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        await _next.DidNotReceive().Invoke(context);
    }

    [Fact]
    public async Task InvokeAsync_WithEmptyHeaderName_UsesDefaultHeaderConstant()
    {
        // Arrange
        var services = new ServiceCollection();
        var wasCalled = false;
        RequestDelegate next = ctx =>
        {
            wasCalled = true;
            return Task.CompletedTask;
        };

        var validator = Substitute.For<IApiKeyValidator>();
        validator.IsValid("xyz456").Returns(Results<bool>.Success(true));

        var options = Options.Create(new ApiSettings
        {
            HeaderName = ""
        });
        services.AddSingleton(validator);
        services.AddSingleton(options);

        var context = new DefaultHttpContext()
        {
            RequestServices = services.BuildServiceProvider()
        };
        context.Request.Headers[Constants.ApiKeyHeaderName] = "xyz456";

        var middleware = new ApiKeyMiddleware(next);

        // Act
        await middleware.InvokeAsync(context);

        // Assert
        Assert.True(wasCalled);
    }

    [Fact]
    public async Task InvokeAsync_WithMinimalApiAllowAnonymous_SkipsValidation()
    {
        // Arrange
        var services = new ServiceCollection();
        var validator = Substitute.For<IApiKeyValidator>();
        var options = Options.Create(new ApiSettings());
        services.AddSingleton(validator);
        services.AddSingleton(options);

        var context = new DefaultHttpContext()
        {
            RequestServices = services.BuildServiceProvider()
        };

        var metadata = new EndpointMetadataCollection();
        context.SetEndpoint(new Endpoint(_ => Task.CompletedTask, metadata, "minimal"));

        var middleware = new ApiKeyMiddleware(_next);

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
