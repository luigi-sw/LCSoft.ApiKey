

using LCSoft.ApiKey.Policy.Authentication;
using Microsoft.Net.Http.Headers;

namespace LCSoft.ApiKey.Tests.Policy.Authentication;

public class ApiKeyAuthenticationPostConfigureOptionsTests
{
    [Fact]
    public void PostConfigure_WhenHeaderNameIsNull_SetsAuthorizationAsDefault()
    {
        // Arrange
        var options = new ApiKeyAuthenticationOptions
        {
            HeaderName = null
        };

        var postConfigure = new ApiKeyAuthenticationPostConfigureOptions();

        // Act
        postConfigure.PostConfigure("ApiKey", options);

        // Assert
        Assert.Equal(HeaderNames.Authorization, options.HeaderName);
    }

    [Fact]
    public void PostConfigure_WhenHeaderNameIsEmpty_SetsAuthorizationAsDefault()
    {
        // Arrange
        var options = new ApiKeyAuthenticationOptions
        {
            HeaderName = ""
        };

        var postConfigure = new ApiKeyAuthenticationPostConfigureOptions();

        // Act
        postConfigure.PostConfigure("ApiKey", options);

        // Assert
        Assert.Equal(HeaderNames.Authorization, options.HeaderName);
    }

    [Fact]
    public void PostConfigure_WhenHeaderNameIsSet_KeepsOriginalValue()
    {
        // Arrange
        var options = new ApiKeyAuthenticationOptions
        {
            HeaderName = "X-Custom-ApiKey"
        };

        var postConfigure = new ApiKeyAuthenticationPostConfigureOptions();

        // Act
        postConfigure.PostConfigure("ApiKey", options);

        // Assert
        Assert.Equal("X-Custom-ApiKey", options.HeaderName);
    }
}
