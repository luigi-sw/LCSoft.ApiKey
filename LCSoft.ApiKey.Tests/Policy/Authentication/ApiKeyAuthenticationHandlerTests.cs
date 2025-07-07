
using LCSoft.ApiKey.Models;
using LCSoft.ApiKey.Policy.Authentication;
using LCSoft.ApiKey.Validation;
using LCSoft.Results;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;
using System.Data;
using System.Security.Claims;
using System.Text.Encodings.Web;
using static System.Formats.Asn1.AsnWriter;

namespace LCSoft.ApiKey.Tests.Policy.Authentication;

public class ApiKeyAuthenticationHandlerTests
{
    private readonly DefaultHttpContext _context;
    private readonly IApiKeyValidator _validator;
    private readonly ApiKeyAuthenticationHandler _handler;

    public ApiKeyAuthenticationHandlerTests()
    {
        _context = new DefaultHttpContext();
        _validator = Substitute.For<IApiKeyValidator>();

        var optionsMonitor = Substitute.For<IOptionsMonitor<ApiKeyAuthenticationOptions>>();
        optionsMonitor.Get(Arg.Any<string>()).Returns(new ApiKeyAuthenticationOptions
        {
            HeaderName = "X-Api-Key"
        });

        var loggerFactory = Substitute.For<ILoggerFactory>();
        var encoder = UrlEncoder.Default;

#if NET6_0 || NET7_0
        var clock = Substitute.For<ISystemClock>();
        _handler = new ApiKeyAuthenticationHandler(optionsMonitor, loggerFactory, encoder, clock, _validator);
#else
        _handler = new ApiKeyAuthenticationHandler(optionsMonitor, loggerFactory, _validator, encoder);
#endif

        _handler.InitializeAsync(new AuthenticationScheme("ApiKey", null, typeof(ApiKeyAuthenticationHandler)), _context);
    }

    [Fact]
    public async Task HandleAuthenticateAsync_WithValidApiKeyHeader_ReturnsSuccess()
    {
        // Arrange
        _context.Request.Headers["X-Api-Key"] = "valid-key";
        _validator.ValidateAndGetInfo("valid-key").Returns(Results<ApiKeyInfo>.Success(new ApiKeyInfo
        {
            Owner = "test-owner",
            Roles = new[] { "Admin" },
            Scopes = new[] { "read" }
        }));

        // Act
        var result = await _handler.AuthenticateAsync();

        // Assert
        Assert.True(result.Succeeded);
        var principal = result.Principal!;
        Assert.Equal("test-owner", principal.Identity?.Name);
        Assert.Contains(principal.Claims, c => c.Type == ClaimTypes.Role && c.Value == "Admin");
        Assert.Contains(principal.Claims, c => c.Type == "scope" && c.Value == "read");
    }

    [Fact]
    public async Task HandleAuthenticateAsync_WithMissingApiKey_ReturnsFail()
    {
        // Act
        var result = await _handler.AuthenticateAsync();

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("Missing API Key", result.Failure?.Message);
    }

    [Fact]
    public async Task HandleAuthenticateAsync_WithAuthorizationWithoutParameter_ReturnsFail()
    {
        _context.Request.Headers["Authorization"] = "ApiKey";

        var result = await _handler.AuthenticateAsync();

        Assert.False(result.Succeeded);
        Assert.Equal("Missing apiKey", result.Failure?.Message);
    }

    [Fact]
    public async Task HandleAuthenticateAsync_WithInvalidKey_ReturnsFail()
    {
        _context.Request.Headers["X-Api-Key"] = "invalid-key";
        _validator.ValidateAndGetInfo("invalid-key").Returns(Results<ApiKeyInfo>.Failure(StandardErrorType.GenericFailure));

        var result = await _handler.AuthenticateAsync();

        Assert.False(result.Succeeded);
        Assert.Equal("Invalid API Key", result.Failure?.Message);
    }

    [Fact]
    public async Task HandleAuthenticateAsync_WithNullInfo_ReturnsFail()
    {
        _context.Request.Headers["X-Api-Key"] = "valid-but-null";
        _validator.ValidateAndGetInfo("valid-but-null").Returns(Results<ApiKeyInfo>.Failure(StandardErrorType.GenericFailure));

        var result = await _handler.AuthenticateAsync();

        Assert.False(result.Succeeded);
        Assert.Equal("Invalid API Key", result.Failure?.Message);
    }

    [Fact]
    public async Task HandleChallengeAsync_WritesUnauthorizedResponse()
    {
        var optionsMonitor = Substitute.For<IOptionsMonitor<ApiKeyAuthenticationOptions>>();
        optionsMonitor.Get(Arg.Any<string>()).Returns(new ApiKeyAuthenticationOptions
        {
            Realm = "MyApi",
            ErrorMessage = "Missing or invalid key"
        });

        var loggerFactory = Substitute.For<ILoggerFactory>();
        var encoder = UrlEncoder.Default;

#if NET6_0 || NET7_0
        var clock = Substitute.For<ISystemClock>();
        var handler = new TestApiKeyAuthenticationHandler(optionsMonitor, loggerFactory, encoder, clock, Substitute.For<IApiKeyValidator>());
#else
    var handler = new TestApiKeyAuthenticationHandler(optionsMonitor, loggerFactory, Substitute.For<IApiKeyValidator>(), encoder);
#endif

        var context = new DefaultHttpContext();
        await handler.InitializeAsync(new AuthenticationScheme("ApiKey", null, typeof(ApiKeyAuthenticationHandler)), context);

        // Act
        await handler.InvokeHandleChallengeAsync(new AuthenticationProperties());

        // Assert
        Assert.Equal(401, context.Response.StatusCode);
        Assert.Equal("application/json", context.Response.ContentType);
        Assert.Equal($"ApiKey realm=\"MyApi\", charset=\"UTF-8\"", context.Response.Headers["WWW-Authenticate"]);
    }

    [Fact]
    public async Task HandleAuthenticateAsync_WithAuthorizationSchemeButNoParameter_ReturnsFail()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers["Authorization"] = "ApiKey";

        context.RequestServices = new ServiceCollection().BuildServiceProvider();

        var optionsMonitor = Substitute.For<IOptionsMonitor<ApiKeyAuthenticationOptions>>();
        optionsMonitor.Get(Arg.Any<string>()).Returns(new ApiKeyAuthenticationOptions()
        {
            HeaderName = "X-Api-Key"
        });

        var loggerFactory = Substitute.For<ILoggerFactory>();
        var encoder = UrlEncoder.Default;
        var validator = Substitute.For<IApiKeyValidator>();

#if NET6_0 || NET7_0
        var clock = Substitute.For<ISystemClock>();
        var handler = new ApiKeyAuthenticationHandler(optionsMonitor, loggerFactory, encoder, clock, validator);
#else
    var handler = new ApiKeyAuthenticationHandler(optionsMonitor, loggerFactory, validator, encoder);
#endif

        await handler.InitializeAsync(new AuthenticationScheme("ApiKey", null, typeof(ApiKeyAuthenticationHandler)), context);

        // Act
        var result = await handler.AuthenticateAsync();

        // Assert
        Assert.False(result.Succeeded);
        Assert.Equal("Missing apiKey", result.Failure?.Message);
    }

    [Fact]
    public async Task HandleAuthenticateAsync_WithAuthorizationHeaderAndValidParameter_ReturnsSuccess()
    {
        // Arrange
        var context = new DefaultHttpContext();
        context.Request.Headers["Authorization"] = "ApiKey my-valid-key";
        context.RequestServices = new ServiceCollection().BuildServiceProvider();

        var validator = Substitute.For<IApiKeyValidator>();
        validator.ValidateAndGetInfo("my-valid-key").Returns(Results<ApiKeyInfo>.Success(new ApiKeyInfo
        {
            Owner = "owner",
            Roles = new[] { "admin" },
            Scopes = new[] { "write" }
        }));

        var optionsMonitor = Substitute.For<IOptionsMonitor<ApiKeyAuthenticationOptions>>();
        optionsMonitor.Get(Arg.Any<string>()).Returns(new ApiKeyAuthenticationOptions()
        {
            HeaderName = "X-Api-Key"
        });

        var loggerFactory = Substitute.For<ILoggerFactory>();
        var encoder = UrlEncoder.Default;

#if NET6_0 || NET7_0
        var clock = Substitute.For<ISystemClock>();
        var handler = new ApiKeyAuthenticationHandler(optionsMonitor, loggerFactory, encoder, clock, validator);
#else
    var handler = new ApiKeyAuthenticationHandler(optionsMonitor, loggerFactory, validator, encoder);
#endif

        await handler.InitializeAsync(new AuthenticationScheme("ApiKey", null, typeof(ApiKeyAuthenticationHandler)), context);

        // Act
        var result = await handler.AuthenticateAsync();

        // Assert
        Assert.True(result.Succeeded);
    }

    [Fact]
    public async Task HandleAuthenticateAsync_WhenApiKeyInfoIsNull_ReturnsFail()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Api-Key"] = "null-key";
        context.RequestServices = new ServiceCollection().BuildServiceProvider();

        var validator = Substitute.For<IApiKeyValidator>();
        validator.ValidateAndGetInfo("null-key").Returns(Results<ApiKeyInfo>.Success(null));

        var optionsMonitor = Substitute.For<IOptionsMonitor<ApiKeyAuthenticationOptions>>();
        optionsMonitor.Get(Arg.Any<string>()).Returns(new ApiKeyAuthenticationOptions
        {
            HeaderName = "X-Api-Key"
        });

        var loggerFactory = Substitute.For<ILoggerFactory>();
        var encoder = UrlEncoder.Default;

#if NET6_0 || NET7_0
        var clock = Substitute.For<ISystemClock>();
        var handler = new ApiKeyAuthenticationHandler(optionsMonitor, loggerFactory, encoder, clock, validator);
#else
    var handler = new ApiKeyAuthenticationHandler(optionsMonitor, loggerFactory, validator, encoder);
#endif

        await handler.InitializeAsync(
            new AuthenticationScheme("ApiKey", null, typeof(ApiKeyAuthenticationHandler)),
            context
        );

        var result = await handler.AuthenticateAsync();

        Assert.False(result.Succeeded);
        Assert.Equal("API Key not found", result.Failure?.Message);
    }

    [Fact]
    public async Task HandleAuthenticateAsync_WhenOwnerIsEmptyDataNull_AssignsUnknown()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers["X-Api-Key"] = "ownerless-key";
        context.RequestServices = new ServiceCollection().BuildServiceProvider();

        var validator = Substitute.For<IApiKeyValidator>();
        validator.ValidateAndGetInfo("ownerless-key").Returns(Results<ApiKeyInfo>.Success(new ApiKeyInfo
        {
            Owner = "", // vazio para forçar fallback
            Roles = null,
            Scopes = null
        }));

        var optionsMonitor = Substitute.For<IOptionsMonitor<ApiKeyAuthenticationOptions>>();
        optionsMonitor.Get(Arg.Any<string>()).Returns(new ApiKeyAuthenticationOptions
        {
            HeaderName = "X-Api-Key"
        });

        var loggerFactory = Substitute.For<ILoggerFactory>();
        var encoder = UrlEncoder.Default;

#if NET6_0 || NET7_0
        var clock = Substitute.For<ISystemClock>();
        var handler = new ApiKeyAuthenticationHandler(optionsMonitor, loggerFactory, encoder, clock, validator);
#else
    var handler = new ApiKeyAuthenticationHandler(optionsMonitor, loggerFactory, validator, encoder);
#endif

        await handler.InitializeAsync(
            new AuthenticationScheme("ApiKey", null, typeof(ApiKeyAuthenticationHandler)),
            context
        );

        var result = await handler.AuthenticateAsync();

        Assert.True(result.Succeeded);
        Assert.Equal("Unknown", result.Principal?.Identity?.Name);
    }


    [Fact]
    public async Task HandleChallengeAsync_WithEmptyRealm_UsesDefaultRealm()
    {
        var options = Substitute.For<IOptionsMonitor<ApiKeyAuthenticationOptions>>();
        options.Get(Arg.Any<string>()).Returns(new ApiKeyAuthenticationOptions
        {
            Realm = "",
            ErrorMessage = "Missing key"
        });

        var loggerFactory = Substitute.For<ILoggerFactory>();
        var encoder = UrlEncoder.Default;
        var validator = Substitute.For<IApiKeyValidator>();

#if NET6_0 || NET7_0
        var clock = Substitute.For<ISystemClock>();
        var handler = new TestApiKeyAuthenticationHandler(options, loggerFactory, encoder, clock, Substitute.For<IApiKeyValidator>());
#else
    var handler = new TestApiKeyAuthenticationHandler(options, loggerFactory, Substitute.For<IApiKeyValidator>(), encoder);
#endif

        var context = new DefaultHttpContext();
        await handler.InitializeAsync(new AuthenticationScheme("ApiKey", null, typeof(ApiKeyAuthenticationHandler)), context);

        // Act
        await handler.InvokeHandleChallengeAsync(new AuthenticationProperties());

        // Assert
        Assert.Equal(401, context.Response.StatusCode);
        Assert.Equal("application/json", context.Response.ContentType);
        Assert.Equal($"ApiKey realm=\"{Constants.DefaultRealm}\", charset=\"UTF-8\"",
                     context.Response.Headers["WWW-Authenticate"]);
    }
}

internal class TestApiKeyAuthenticationHandler : ApiKeyAuthenticationHandler
{
#if NET6_0 || NET7_0
    public TestApiKeyAuthenticationHandler(
        IOptionsMonitor<ApiKeyAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        IApiKeyValidator apiKeyValidator)
        : base(options, logger, encoder, clock, apiKeyValidator) { }
#else
    public TestApiKeyAuthenticationHandler(
        IOptionsMonitor<ApiKeyAuthenticationOptions> options,
        ILoggerFactory logger,
        IApiKeyValidator apiKeyValidator,
        UrlEncoder encoder)
        : base(options, logger, apiKeyValidator, encoder) { }
#endif

    public async Task InvokeHandleChallengeAsync(AuthenticationProperties properties)
    {
        await HandleChallengeAsync(properties);
    }
}