using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using LCSoft.ApiKey.Validation;
using LCSoft.ApiKey.Models;
using Microsoft.Net.Http.Headers;
using LCSoft.ApiKey.Attribute;
using Microsoft.AspNetCore.Routing;
using System.Net.Http.Headers;
using LCSoft.Results;
using System.Net.Http;

namespace LCSoft.ApiKey.Tests.AttributeTests;

public class CustomAuthorizationTests
{
    private static AuthorizationFilterContext CreateContext(
        IApiKeyValidator? validator = null,
        IOptions<ApiSettings>? options = null,
        Dictionary<string, string>? headers = null)
    {
        var httpContext = new DefaultHttpContext();

        // Adiciona headers simulados
        if (headers != null)
        {
            foreach (var kv in headers)
            {
                httpContext.Request.Headers[kv.Key] = kv.Value;
            }
        }

        // Configura DI
        var services = Substitute.For<IServiceProvider>();
        services.GetService(typeof(IOptions<ApiSettings>)).Returns(options);
        services.GetService(typeof(IApiKeyValidator)).Returns(validator);
        httpContext.RequestServices = services;

        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()
        );

        return new AuthorizationFilterContext(actionContext, new List<IFilterMetadata>());
    }

    [Fact]
    public void AuthorizationHeader_ValidApiKey_Passes()
    {
        // Arrange
        var validator = Substitute.For<IApiKeyValidator>();
        validator.IsValid("valid-token").Returns(Results<bool>.Success(true));

        var headers = new Dictionary<string, string>
        {
            { HeaderNames.Authorization, "valid-token" }
        };

        var context = CreateContext(validator, headers: headers);
        var attribute = new CustomAuthorization();

        // Act
        attribute.OnAuthorization(context);

        // Assert
        Assert.Null(context.Result);
    }

    [Fact]
    public void AuthorizationHeader_InvalidApiKey_ReturnsForbidden()
    {
        var validator = Substitute.For<IApiKeyValidator>();
        validator.IsValid("bad-token").Returns(Results<bool>.Failure(StandardErrorType.GenericFailure));

        var headers = new Dictionary<string, string>
        {
            { HeaderNames.Authorization, "bad-token" }
        };

        var context = CreateContext(validator, headers: headers);
        var attribute = new CustomAuthorization();

        attribute.OnAuthorization(context);

        var result = Assert.IsType<JsonResult>(context.Result);
        dynamic value = result.Value!;
        Assert.Equal("Error", value.Status);
        Assert.Contains("Unauthorized", (string)value.Message);
    }

    [Fact]
    public void NoAuthorizationHeader_InvalidApiKeyHeader_ReturnsForbidden()
    {
        var validator = Substitute.For<IApiKeyValidator>();
        validator.IsValid("invalid-api-key").Returns(Results<bool>.Failure(StandardErrorType.GenericFailure));

        var headers = new Dictionary<string, string>
        {
            { Constants.ApiKeyHeaderName, "invalid-api-key" }
        };

        var context = CreateContext(validator, headers: headers);
        var attribute = new CustomAuthorization();

        attribute.OnAuthorization(context);

        var result = Assert.IsType<JsonResult>(context.Result);
        dynamic value = result.Value!;
        Assert.Equal("Error", value.Status);
        Assert.Contains("Please Provide", (string)value.Message);
    }

    [Fact]
    public void NoAuthorizationHeader_ValidApiKeyHeader_Passes()
    {
        var validator = Substitute.For<IApiKeyValidator>();
        validator.IsValid("my-valid-api-key").Returns(Results<bool>.Success(true));

        var headers = new Dictionary<string, string>
        {
            { Constants.ApiKeyHeaderName, "my-valid-api-key" }
        };

        var context = CreateContext(validator, headers: headers);
        var attribute = new CustomAuthorization();

        attribute.OnAuthorization(context);

        Assert.Null(context.Result);
    }

    [Fact]
    public void AuthorizationHeaderEmpty_InvalidApiKey_ReturnsForbidden()
    {
        var validator = Substitute.For<IApiKeyValidator>();
        validator.IsValid("invalid").Returns(Results<bool>.Failure(StandardErrorType.GenericFailure));

        var headers = new Dictionary<string, string>
        {
            { HeaderNames.Authorization, "" },
            { Constants.ApiKeyHeaderName, "invalid" }
        };

        var context = CreateContext(validator, headers: headers);
        var attribute = new CustomAuthorization();

        attribute.OnAuthorization(context);

        var result = Assert.IsType<JsonResult>(context.Result);
        dynamic value = result.Value!;
        Assert.Equal("Error", value.Status);
        Assert.Contains("Please Provide", (string)value.Message);
    }

    [Fact]
    public void AuthorizationHeader_WithApiKeyScheme_InvalidToken_ReturnsForbidden()
    {
        var validator = Substitute.For<IApiKeyValidator>();
        validator.IsValid("invalid-token").Returns(Results<bool>.Failure(StandardErrorType.GenericFailure));

        var authHeader = new AuthenticationHeaderValue(Constants.ApiKeyName, "invalid-token").ToString();

        var headers = new Dictionary<string, string>
        {
            { HeaderNames.Authorization, authHeader }
        };

        var context = CreateContext(validator, headers: headers);
        var attribute = new CustomAuthorization();

        attribute.OnAuthorization(context);

        var result = Assert.IsType<JsonResult>(context.Result);
        dynamic value = result.Value!;
        Assert.Equal("Error", value.Status);
        Assert.Equal("The Api key is incorrect : Unauthorized access", (string)value.Message);
    }

    [Fact]
    public void AuthorizationHeader_WithApiKeyScheme_ValidToken_Passes()
    {
        // Arrange
        var validator = Substitute.For<IApiKeyValidator>();
        validator.IsValid("valid-token").Returns(Results<bool>.Success(true));

        var authHeaderValue = new AuthenticationHeaderValue(Constants.ApiKeyName, "valid-token");

        var headers = new Dictionary<string, string>
        {
            { HeaderNames.Authorization, authHeaderValue.ToString() } // ex: "ApiKey valid-token"
        };

        var context = CreateContext(validator, headers: headers);
        var attribute = new CustomAuthorization();

        // Act
        attribute.OnAuthorization(context);

        // Assert
        Assert.Null(context.Result);
    }

    [Fact]
    public void AuthorizationHeader_ApiKeySchemeWithoutValue_FallbacksToApiKeyHeader()
    {
        var validator = Substitute.For<IApiKeyValidator>();

        var authHeader = $"{Constants.ApiKeyName} "; // vazio
        validator.IsValid("fallback-key").Returns(Results<bool>.Success(true));

        var headers = new Dictionary<string, string>
        {
            { HeaderNames.Authorization, authHeader },
            { Constants.ApiKeyHeaderName, "fallback-key" }
        };

        var context = CreateContext(validator, headers: headers);
        var attribute = new CustomAuthorization();

        attribute.OnAuthorization(context);

        Assert.Null(context.Result);
    }

    [Fact]
    public void AuthorizationHeader_WithApiKeySchemeButEmptyValue_ReturnsForbidden()
    {
        var validator = Substitute.For<IApiKeyValidator>();
        validator.IsValid(Arg.Any<string>()).Returns(Results<bool>.Failure(StandardErrorType.GenericFailure));

        var authHeader = new System.Net.Http.Headers.AuthenticationHeaderValue(Constants.ApiKeyName, "").ToString();

        var headers = new Dictionary<string, string>
        {
            { HeaderNames.Authorization, authHeader }
            // Sem ApiKeyHeader para fallback
        };

        var context = CreateContext(validator, headers: headers);
        var attribute = new CustomAuthorization();

        attribute.OnAuthorization(context);

        var result = Assert.IsType<JsonResult>(context.Result);
        dynamic value = result.Value!;
        Assert.Equal("Error", value.Status);
        Assert.Contains("Please Provide", (string)value.Message);
    }

    [Fact]
    public void AuthorizationHeader_FilterContexNull_ReturnsForbidden()
    {
        var httpContext = new DefaultHttpContext();
        var actionContext = new ActionContext(
            httpContext,
            new RouteData(),
            new Microsoft.AspNetCore.Mvc.Abstractions.ActionDescriptor()
        );

        var context = new AuthorizationFilterContext(actionContext, new List<IFilterMetadata>());

        var attribute = new CustomAuthorization();

        attribute.OnAuthorization(null);

        Assert.Null(context.Result);
    }

    [Fact]
    public void AuthorizationHeader_WithNullOptions_ReturnsForbidden()
    {
        var validator = Substitute.For<IApiKeyValidator>();
        validator.IsValid(Arg.Any<string>()).Returns(Results<bool>.Failure(StandardErrorType.GenericFailure));

        var authHeader = new System.Net.Http.Headers.AuthenticationHeaderValue(Constants.ApiKeyName, "").ToString();

        var headers = new Dictionary<string, string>
        {
            { HeaderNames.Authorization, authHeader }
        };
        var options = Substitute.For<IOptions<ApiSettings>>();
        options.Value.Returns(new ApiSettings
        {
            HeaderName = "X-Api-Key"
        });

        var context = CreateContext(validator, options, headers: headers);
        var attribute = new CustomAuthorization();

        attribute.OnAuthorization(context);

        var result = Assert.IsType<JsonResult>(context.Result);
        dynamic value = result.Value!;
        Assert.Equal("Error", value.Status);
        Assert.Contains("Please Provide", (string)value.Message);
    }

    [Fact]
    public void AuthorizationHeader_WithNotNullHeader_ReturnsForbidden()
    {
        var validator = Substitute.For<IApiKeyValidator>();
        validator.IsValid(Arg.Any<string>()).Returns(Results<bool>.Failure(StandardErrorType.GenericFailure));

        var authHeader = new System.Net.Http.Headers.AuthenticationHeaderValue(Constants.ApiKeyName, "").ToString();

        var context = CreateContext(validator);
        var attribute = new CustomAuthorization() { 
            AuthorizationHeader = "Header",
            ApiKeyHeader = "ApiHeader"
        };

        attribute.OnAuthorization(context);

        var result = Assert.IsType<JsonResult>(context.Result);
        dynamic value = result.Value!;
        Assert.Equal("Error", value.Status);
        Assert.Contains("Please Provide", (string)value.Message);
    }

    [Fact]
    public void ApiKeyHeader_ValidApiKey_Passes()
    {
        // Arrange
        var validator = Substitute.For<IApiKeyValidator>();
        validator.IsValid("valid-token").Returns(Results<bool>.Success(true));

        var headers = new Dictionary<string, string>
        {
            { Constants.ApiKeyHeaderName, "" }
        };

        var options = Substitute.For<IOptions<ApiSettings>>();
        options.Value.Returns(new ApiSettings
        {
            HeaderName = "X-Api-Key"
        });

        var context = CreateContext(validator, options, headers);
        var attribute = new CustomAuthorization();

        // Act
        attribute.OnAuthorization(context);

        // Assert
        var result = Assert.IsType<JsonResult>(context.Result);
        dynamic value = result.Value!;
        Assert.Equal("Error", value.Status);
        Assert.Contains("Please Provide", (string)value.Message);
    }
}