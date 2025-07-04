﻿using LCSoft.ApiKey.Attribute;
using LCSoft.ApiKey.Models;
using LCSoft.ApiKey.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System.Net.Http.Headers;
using NSubstitute;

namespace LCSoft.ApiKey.Tests;

public class ApiKeyAuthorizationFilterTests
{
    private const string ValidApiKey = "valid-api-key";
    private const string CustomHeaderName = "X-Custom-ApiKey";

    private static ApiKeyAuthorizationFilter CreateFilter(
        string? headerName = null,
        bool isValid = true)
    {
        var apiKeyValidator = Substitute.For<IApiKeyValidator>();
        apiKeyValidator.IsValid(Arg.Any<string>()).Returns(isValid);

        var options = Substitute.For<IOptions<ApiSettings>>();
        options.Value.Returns(new ApiSettings
        {
            HeaderName = headerName
        });

        return new ApiKeyAuthorizationFilter(apiKeyValidator, options);
    }

    private static AuthorizationFilterContext CreateContextWithHeaders(Dictionary<string, string> headers)
    {
        var httpContext = new DefaultHttpContext();

        foreach (var header in headers)
        {
            httpContext.Request.Headers[header.Key] = header.Value;
        }

        var actionContext = new ActionContext
        {
            HttpContext = httpContext,
            RouteData = new Microsoft.AspNetCore.Routing.RouteData(),
            ActionDescriptor = new Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor()
        };

        return new AuthorizationFilterContext(actionContext, new List<IFilterMetadata>());
    }

    [Fact]
    public void Valid_ApiKey_In_CustomHeader_Should_Pass()
    {
        // Arrange
        var filter = CreateFilter(headerName: CustomHeaderName, isValid: true);

        var context = CreateContextWithHeaders(new()
        {
            { CustomHeaderName, ValidApiKey }
        });

        // Act
        filter.OnAuthorization(context);

        // Assert
        Assert.Null(context.Result);
    }

    [Fact]
    public void Invalid_ApiKey_In_CustomHeader_Should_Return_Unauthorized()
    {
        // Arrange
        var filter = CreateFilter(headerName: CustomHeaderName, isValid: false);

        var context = CreateContextWithHeaders(new()
        {
            { CustomHeaderName, "wrong-api-key" }
        });

        // Act
        filter.OnAuthorization(context);

        // Assert
        Assert.IsType<UnauthorizedResult>(context.Result);
    }

    [Fact]
    public void Valid_ApiKey_In_AuthorizationHeader_Should_Pass()
    {
        // Arrange
        var filter = CreateFilter(headerName: "MissingHeader", isValid: true);

        var authHeaderValue = new AuthenticationHeaderValue("ApiKey", ValidApiKey).ToString();

        var context = CreateContextWithHeaders(new()
        {
            { HeaderNames.Authorization, authHeaderValue }
        });

        // Act
        filter.OnAuthorization(context);

        // Assert
        Assert.Null(context.Result);
    }

    [Fact]
    public void Missing_ApiKey_Should_Return_Unauthorized()
    {
        // Arrange
        var filter = CreateFilter(headerName: CustomHeaderName);

        var context = CreateContextWithHeaders(new());

        // Act
        filter.OnAuthorization(context);

        // Assert
        Assert.IsType<UnauthorizedResult>(context.Result);
    }

    [Fact]
    public void AuthorizationHeader_With_Invalid_Scheme_Should_Return_Unauthorized()
    {
        // Arrange
        var filter = CreateFilter();

        var invalidAuthHeader = new AuthenticationHeaderValue("Bearer", ValidApiKey).ToString();

        var context = CreateContextWithHeaders(new()
        {
            { HeaderNames.Authorization, invalidAuthHeader }
        });

        // Act
        filter.OnAuthorization(context);

        // Assert
        Assert.IsType<UnauthorizedResult>(context.Result);
    }
}
