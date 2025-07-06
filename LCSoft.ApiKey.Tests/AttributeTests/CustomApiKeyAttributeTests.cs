using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NSubstitute;
using System.Net.Http.Headers;
using LCSoft.ApiKey.Validation;
using LCSoft.ApiKey.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Routing;
using LCSoft.ApiKey.Attribute;


namespace LCSoft.ApiKey.Tests.AttributeTests;

public class CustomApiKeyAttributeTests
{
    private static ActionExecutingContext CreateContext(
        IApiKeyValidator? apiKeyValidator = null,
        IOptions<ApiSettings>? options = null,
        Dictionary<string, string>? headers = null)
    {
        var httpContext = new DefaultHttpContext();
        httpContext.RequestServices = Substitute.For<IServiceProvider>();

        // Headers
        if (headers != null)
        {
            foreach (var kvp in headers)
            {
                httpContext.Request.Headers[kvp.Key] = kvp.Value;
            }
        }

        // Service setup
        httpContext.RequestServices
            .GetService(typeof(IOptions<ApiSettings>))
            .Returns(options);

        var serviceProvider = Substitute.For<IServiceProvider>();
        serviceProvider.GetService(typeof(IApiKeyValidator)).Returns(apiKeyValidator);
        serviceProvider.GetService(typeof(IOptions<ApiSettings>)).Returns(options);
        httpContext.RequestServices = serviceProvider;

        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()
        );

        var context = new ActionExecutingContext(
            actionContext,
            new List<IFilterMetadata>(),
            new Dictionary<string, object>(),
            controller: null
        );

        return context;
    }

    [Fact]
    public async Task OnActionExecutionAsync_ValidApiKey_AllowsExecution()
    {
        // Arrange
        var apiKeyValidator = Substitute.For<IApiKeyValidator>();
        apiKeyValidator.IsValid("valid-key").Returns(true);

        var context = CreateContext(
            apiKeyValidator: apiKeyValidator,
            headers: new() { { Constants.ApiKeyHeaderName, "valid-key" } }
        );

        var executed = false;
        var attribute = new CustomApiKeyAttribute();

        // Act
        await attribute.OnActionExecutionAsync(context, () =>
        {
            executed = true;
            return Task.FromResult<ActionExecutedContext>(null);
        });

        // Assert
        Assert.True(executed);
        Assert.Null(context.Result);
    }

    [Fact]
    public async Task OnActionExecutionAsync_MissingApiKeyHeader_Returns401()
    {
        // Arrange
        var apiKeyValidator = Substitute.For<IApiKeyValidator>();
        var context = CreateContext(apiKeyValidator: apiKeyValidator);

        var attribute = new CustomApiKeyAttribute();

        // Act
        await attribute.OnActionExecutionAsync(context, () => Task.FromResult<ActionExecutedContext>(null));

        // Assert
        var result = Assert.IsType<ContentResult>(context.Result);
        Assert.Equal(401, result.StatusCode);
        Assert.Contains("not available", result.Content);
    }

    [Fact]
    public async Task OnActionExecutionAsync_InvalidApiKey_Returns401()
    {
        // Arrange
        var apiKeyValidator = Substitute.For<IApiKeyValidator>();
        apiKeyValidator.IsValid("invalid").Returns(false);

        var context = CreateContext(
            apiKeyValidator: apiKeyValidator,
            headers: new() { { Constants.ApiKeyHeaderName, "invalid" } }
        );

        var attribute = new CustomApiKeyAttribute();

        // Act
        await attribute.OnActionExecutionAsync(context, () => Task.FromResult<ActionExecutedContext>(null));

        // Assert
        var result = Assert.IsType<ContentResult>(context.Result);
        Assert.Equal(401, result.StatusCode);
        Assert.Contains("incorrect", result.Content);
    }

    [Fact]
    public async Task OnActionExecutionAsync_UsesCustomHeader_FromConstructor()
    {
        // Arrange
        var apiKeyValidator = Substitute.For<IApiKeyValidator>();
        apiKeyValidator.IsValid("custom-key").Returns(true);

        var context = CreateContext(
            apiKeyValidator: apiKeyValidator,
            headers: new() { { "My-Header", "custom-key" } }
        );

        var attribute = new CustomApiKeyAttribute("My-Header");

        // Act
        await attribute.OnActionExecutionAsync(context, () => Task.FromResult<ActionExecutedContext>(null));

        // Assert
        Assert.Null(context.Result);
    }

    [Fact]
    public async Task OnActionExecutionAsync_UsesAuthorizationHeader_WithScheme()
    {
        // Arrange
        var apiKeyValidator = Substitute.For<IApiKeyValidator>();
        apiKeyValidator.IsValid("auth-key").Returns(true);

        var authHeader = new AuthenticationHeaderValue(Constants.ApiKeyName, "auth-key").ToString();

        var context = CreateContext(
            apiKeyValidator: apiKeyValidator,
            headers: new() { { "Authorization", authHeader } }
        );

        var attribute = new CustomApiKeyAttribute();

        // Act
        await attribute.OnActionExecutionAsync(context, () => Task.FromResult<ActionExecutedContext>(null));

        // Assert
        Assert.Null(context.Result);
    }

    [Fact]
    public async Task OnActionExecutionAsync_HeaderPresentButEmpty_Returns401WithUnauthorizedMessage()
    {
        // Arrange
        var apiKeyValidator = Substitute.For<IApiKeyValidator>();

        var context = CreateContext(
            apiKeyValidator: apiKeyValidator,
            headers: new() { { Constants.ApiKeyHeaderName, "" } } // <-- chave presente mas vazia
        );

        var attribute = new CustomApiKeyAttribute();

        // Act
        await attribute.OnActionExecutionAsync(context, () => Task.FromResult<ActionExecutedContext>(null));

        // Assert
        var result = Assert.IsType<ContentResult>(context.Result);
        Assert.Equal(401, result.StatusCode);
        Assert.Equal("The Api key is incorrect : Unauthorized access", result.Content);
    }
}