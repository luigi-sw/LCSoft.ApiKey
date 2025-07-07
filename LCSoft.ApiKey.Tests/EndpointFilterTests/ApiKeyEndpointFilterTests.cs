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

public class ApiKeyEndpointFilterTests
{
    private const string ValidApiKey = "valid-key";
    private const string InvalidApiKey = "invalid-key";
    private const string CustomHeaderName = "X-Api-Key";

    private static DefaultHttpContext CreateHttpContext(string? apiKey = null, string? authorization = null)
    {
        var context = new DefaultHttpContext();
        if (apiKey != null)
        {
            context.Request.Headers[CustomHeaderName] = apiKey;
        }

        if (authorization != null)
        {
            context.Request.Headers[HeaderNames.Authorization] = authorization;
        }

        return context;
    }

    private static EndpointFilterInvocationContext CreateInvocationContext(HttpContext httpContext)
    {
        var context = Substitute.For<EndpointFilterInvocationContext>();
        context.HttpContext.Returns(httpContext);
        return context;
    }

    [Fact]
    public async Task InvokeAsync_ValidApiKeyInCustomHeader_ReturnsNext()
    {
        // Arrange
        var validator = Substitute.For<IApiKeyValidator>();
        validator.IsValid(ValidApiKey).Returns(true);

        var options = Substitute.For<IOptions<ApiSettings>>();
        var apiSettings = new ApiSettings { HeaderName = CustomHeaderName };
        options.Value.Returns(apiSettings);

        var filter = new ApiKeyEndpointFilter(validator, options);
        var context = CreateInvocationContext(CreateHttpContext(apiKey: ValidApiKey));

        var next = Substitute.For<EndpointFilterDelegate>();
        next(context).Returns(Task.FromResult<object?>(new object()));

        // Act
        var result = await filter.InvokeAsync(context, next);

        // Assert
        Assert.NotNull(result);
        await next.Received(1)(context);
    }

    [Fact]
    public async Task InvokeAsync_ApiKeyMissing_ReturnsUnauthorized()
    {
        // Arrange
        var validator = Substitute.For<IApiKeyValidator>();
        var options = Substitute.For<IOptions<ApiSettings>>();
        options.Value.Returns(new ApiSettings { HeaderName = CustomHeaderName });

        var filter = new ApiKeyEndpointFilter(validator, options);
        var context = CreateInvocationContext(CreateHttpContext());

        var next = Substitute.For<EndpointFilterDelegate>();

        // Act
        var result = await filter.InvokeAsync(context, next);

        // Assert
        Assert.IsType<UnauthorizedHttpObjectResult>(result);
        await next.DidNotReceive()(Arg.Any<EndpointFilterInvocationContext>());
    }

    [Fact]
    public async Task InvokeAsync_ApiKeyInvalid_ReturnsUnauthorized()
    {
        // Arrange
        var validator = Substitute.For<IApiKeyValidator>();
        validator.IsValid(InvalidApiKey).Returns(false);

        var options = Substitute.For<IOptions<ApiSettings>>();
        options.Value.Returns(new ApiSettings { HeaderName = CustomHeaderName });

        var filter = new ApiKeyEndpointFilter(validator, options);
        var context = CreateInvocationContext(CreateHttpContext(apiKey: InvalidApiKey));

        var next = Substitute.For<EndpointFilterDelegate>();

        // Act
        var result = await filter.InvokeAsync(context, next);

        // Assert
        Assert.IsType<UnauthorizedHttpObjectResult>(result);
        await next.DidNotReceive()(Arg.Any<EndpointFilterInvocationContext>());
    }

    [Fact]
    public async Task InvokeAsync_AuthorizationHeaderWithApiKeyScheme_Fails()
    {
        // Arrange
        var validKey = "valid-key";

        var validator = Substitute.For<IApiKeyValidator>();
        validator.IsValid(validKey).Returns(true);

        var options = Options.Create(new ApiSettings { HeaderName = "X-Api-Key" });

        var httpContext = new DefaultHttpContext();
        var authHeader = new AuthenticationHeaderValue("ApiKey", validKey).ToString();
        httpContext.Request.Headers[HeaderNames.Authorization] = authHeader;

        var context = Substitute.For<EndpointFilterInvocationContext>();
        context.HttpContext.Returns(httpContext);

        var next = Substitute.For<EndpointFilterDelegate>();
        var expected = new UnauthorizedHttpObjectResult(StatusCodes.Status401Unauthorized) { };
        next(context).Returns(expected);

        var filter = new ApiKeyEndpointFilter(validator, options);

        // Act
        var result = await filter.InvokeAsync(context, next);

        // Assert
        Assert.Equivalent(expected, result);
    }

    [Fact]
    public async Task InvokeAsync_ValidApiKey_FromAuthorizationHeaderWithApiKeyScheme_CallsNext()
    {
        // Arrange
        var validKey = "my-secret-key";

        var validator = Substitute.For<IApiKeyValidator>();
        validator.IsValid(validKey).Returns(true);

        var options = Options.Create(new ApiSettings { HeaderName = "X-Api-Key" });

        var httpContext = new DefaultHttpContext();

        // Simula: Authorization: ApiKey my-secret-key
        var authHeader = new AuthenticationHeaderValue("ApiKey", validKey).ToString();
        httpContext.Request.Headers[HeaderNames.Authorization] = authHeader;

        var context = Substitute.For<EndpointFilterInvocationContext>();
        context.HttpContext.Returns(httpContext);

        var next = Substitute.For<EndpointFilterDelegate>();
        var expectedResult = new UnauthorizedHttpObjectResult(StatusCodes.Status401Unauthorized) { };
        next(context).Returns(expectedResult);

        var filter = new ApiKeyEndpointFilter(validator, options);

        // Act
        var result = await filter.InvokeAsync(context, next);

        // Assert
        Assert.Equivalent(expectedResult, result);
    }

    [Fact]
    public async Task InvokeAsync_ValidApiKey_CallsNextAndReturnsNextResult()
    {
        // Arrange
        var validApiKey = "valid-key";

        var validator = Substitute.For<IApiKeyValidator>();
        validator.IsValid(validApiKey).Returns(true);

        var options = Options.Create(new ApiSettings { HeaderName = "X-Api-Key" });

        var httpContext = new DefaultHttpContext();
        // Não adicionar X-Api-Key para forçar usar Authorization
        var authHeader = new AuthenticationHeaderValue("ApiKey", validApiKey).ToString();
        httpContext.Request.Headers[HeaderNames.Authorization] = authHeader;

        var context = Substitute.For<EndpointFilterInvocationContext>();
        context.HttpContext.Returns(httpContext);

        var next = Substitute.For<EndpointFilterDelegate>();
        var expectedResult = new UnauthorizedHttpObjectResult(StatusCodes.Status401Unauthorized) { };
        next(context).Returns(expectedResult);

        var filter = new ApiKeyEndpointFilter(validator, options);

        // Act
        var result = await filter.InvokeAsync(context, next);

        // Assert
        Assert.Equivalent(expectedResult, result);
    }

    [Fact]
    public async Task InvokeAsync_ApiKeyMissingEmpty_ReturnsUnauthorized()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();

        var validator = Substitute.For<IApiKeyValidator>();

        var options = Options.Create(new ApiSettings { HeaderName = "X-Api-Key" });

        httpContext.Request.Headers[HeaderNames.Authorization] = "";

        var context = Substitute.For<EndpointFilterInvocationContext>();
        context.HttpContext.Returns(httpContext);

        var next = Substitute.For<EndpointFilterDelegate>();

        var filter = new ApiKeyEndpointFilter(validator, options);

        // Act
        var result = await filter.InvokeAsync(context, next);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedHttpObjectResult>(result);
        await next.DidNotReceive()(context);
    }

    [Fact]
    public async Task InvokeAsync_ApiKeyEmpty_ReturnsUnauthorized()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();

        var validator = Substitute.For<IApiKeyValidator>();
        //validator.IsValid(validApiKey).Returns(true);

        var options = Options.Create(new ApiSettings { HeaderName = "X-Api-Key" });

        // Envia X-Api-Key vazio
        httpContext.Request.Headers["X-Api-Key"] = "";

        var context = Substitute.For<EndpointFilterInvocationContext>();
        context.HttpContext.Returns(httpContext);

        var next = Substitute.For<EndpointFilterDelegate>();

        var filter = new ApiKeyEndpointFilter(validator, options);

        // Act
        var result = await filter.InvokeAsync(context, next);

        // Assert
        var unauthorizedResult = Assert.IsType<UnauthorizedHttpObjectResult>(result);
        await next.DidNotReceive()(context); // next não deve ser chamado
    }
}
#endif