
using LCSoft.ApiKey.Policy.Auhtorization;
using LCSoft.ApiKey.Validation;
using LCSoft.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using NSubstitute;

namespace LCSoft.ApiKey.Tests.Policy.Authorization;

public class ApiKeyAuthorizationHandlerTests
{
    private readonly IApiKeyValidator _validator;
    private readonly ApiKeyAuthorizationHandler _handler;
    private readonly ApiKeyRequirement _requirement;

    public ApiKeyAuthorizationHandlerTests()
    {
        _validator = Substitute.For<IApiKeyValidator>();
        _handler = new ApiKeyAuthorizationHandler(_validator);
        _requirement = new ApiKeyRequirement();
    }

    private AuthorizationHandlerContext CreateContext(HttpContext httpContext)
    {
        return new AuthorizationHandlerContext(
            requirements: new[] { _requirement },
            user: null!,
            resource: httpContext);
    }

    [Fact]
    public void Succeed_When_ApiKeyHeader_IsValid()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers[Constants.ApiKeyHeaderName] = "valid-key";
        _validator.IsValid("valid-key").Returns(Results<bool>.Success(true));

        var context = CreateContext(httpContext);

        // Act
        var result = _handler.SucceedRequirementIfApiKeyPresentAndValid(context, _requirement);

        // Assert
        Assert.True(result);
        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public void Succeed_When_AuthorizationHeader_WithApiKey_IsValid()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Authorization"] = "ApiKey valid-token";
        _validator.IsValid("valid-token").Returns(Results<bool>.Success(true));

        var context = CreateContext(httpContext);

        // Act
        var result = _handler.SucceedRequirementIfApiKeyPresentAndValid(context, _requirement);

        // Assert
        Assert.True(result);
        Assert.True(context.HasSucceeded);
    }

    [Fact]
    public void Fail_When_ApiKeyHeader_IsMissing()
    {
        // Arrange
        var httpContext = new DefaultHttpContext(); // No headers
        var context = CreateContext(httpContext);

        // Act
        var result = _handler.SucceedRequirementIfApiKeyPresentAndValid(context, _requirement);

        // Assert
        Assert.False(result);
        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public void Fail_When_ApiKeyHeader_IsInvalid()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers[Constants.ApiKeyHeaderName] = "invalid-key";
        _validator.IsValid("invalid-key").Returns(Results<bool>.Failure(StandardErrorType.GenericFailure));

        var context = CreateContext(httpContext);

        // Act
        var result = _handler.SucceedRequirementIfApiKeyPresentAndValid(context, _requirement);

        // Assert
        Assert.False(result);
        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public void Fail_When_ContextResource_IsNotHttpContext()
    {
        // Arrange
        var context = new AuthorizationHandlerContext(
            requirements: new[] { _requirement },
            user: null!,
            resource: new object()); // Not HttpContext

        // Act
        var result = _handler.SucceedRequirementIfApiKeyPresentAndValid(context, _requirement);

        // Assert
        Assert.False(result);
        Assert.False(context.HasSucceeded);
    }

    [Fact]
    public void Fail_When_AuthorizationHeader_IsMalformed()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers["Authorization"] = "BadHeaderWithoutSpace";

        var context = CreateContext(httpContext);

        // Act
        var result = _handler.SucceedRequirementIfApiKeyPresentAndValid(context, _requirement);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public async Task HandleRequirementAsync_WithValidApiKey_SucceedsRequirement()
    {
        // Arrange
        var httpContext = new DefaultHttpContext();
        httpContext.Request.Headers[Constants.ApiKeyHeaderName] = "valid-key";

        var validator = Substitute.For<IApiKeyValidator>();
        validator.IsValid("valid-key").Returns(Results<bool>.Success(true));

        var handler = new ApiKeyAuthorizationHandler(validator);
        var requirement = new ApiKeyRequirement();

        var context = new AuthorizationHandlerContext(
            new[] { requirement },
            user: null!,
            resource: httpContext);

        // Act
        await handler.HandleAsync(context); // <- chama HandleRequirementAsync internamente

        // Assert
        Assert.True(context.HasSucceeded);
    }
}
