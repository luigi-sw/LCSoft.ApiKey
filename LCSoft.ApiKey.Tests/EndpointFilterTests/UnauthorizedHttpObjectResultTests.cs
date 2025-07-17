#if NET7_0_OR_GREATER
using LCSoft.ApiKey.EndpointFilter;
using Microsoft.AspNetCore.Http;
using System.Text.Json;

namespace LCSoft.ApiKey.Tests.EndpointFilterTests;

public class UnauthorizedHttpObjectResultTests
{
    [Fact]
    public void StatusCode_Is401()
    {
        var result = new UnauthorizedHttpObjectResult("any");
        Assert.Equal(StatusCodes.Status401Unauthorized, result.StatusCode);
        Assert.Equal(StatusCodes.Status401Unauthorized, ((IStatusCodeHttpResult)result).StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_WritesStringBody()
    {
        // Arrange
        var body = "Unauthorized access";
        var result = new UnauthorizedHttpObjectResult(body);

        var context = new DefaultHttpContext();
        var responseBodyStream = new MemoryStream();
        context.Response.Body = responseBodyStream;

        // Act
        await result.ExecuteAsync(context);

        // Assert
        responseBodyStream.Seek(0, SeekOrigin.Begin);
        var reader = new StreamReader(responseBodyStream);
        var responseText = await reader.ReadToEndAsync();

        Assert.Equal(body, responseText);
        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_WritesJsonBody_WhenBodyIsObject()
    {
        // Arrange
        var body = new { message = "Unauthorized", code = 401 };
        var result = new UnauthorizedHttpObjectResult(body);

        var context = new DefaultHttpContext();
        var responseBodyStream = new MemoryStream();
        context.Response.Body = responseBodyStream;

        // Act
        await result.ExecuteAsync(context);

        // Assert
        responseBodyStream.Seek(0, SeekOrigin.Begin);
        var json = await JsonSerializer.DeserializeAsync<JsonElement>(responseBodyStream);

        Assert.Equal("Unauthorized", json.GetProperty("message").GetString());
        Assert.Equal(401, json.GetProperty("code").GetInt32());
        Assert.Equal(StatusCodes.Status401Unauthorized, context.Response.StatusCode);
    }

    [Fact]
    public async Task ExecuteAsync_ThrowsArgumentNullException_WhenHttpContextIsNull()
    {
        var result = new UnauthorizedHttpObjectResult("body");

        await Assert.ThrowsAsync<System.ArgumentNullException>(() => result.ExecuteAsync(null!));
    }
}
#endif