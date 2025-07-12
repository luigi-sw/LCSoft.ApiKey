using LCSoft.ApiKey.Models;
using LCSoft.ApiKey.Validation;
using LCSoft.Results;
using Microsoft.Extensions.Options;
using System.Text;
using System.Text.Json;

namespace LCSoft.ApiKey.Tests.Validation;

public class DefaultApiKeyStrategyTests
{
    private DefaultApiKeyStrategy CreateStrategy(IEnumerable<string>? keys = null)
    {
        var options = Options.Create(new DefaultApiKeyStrategyOptions
        {
            ApiKeys = keys?.ToList() ?? new List<string>()
        });

        return new DefaultApiKeyStrategy(options);
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenApiKeyIsNullOrWhitespace()
    {
        var strategy = CreateStrategy(new[] { "key123" });

        Assert.False(strategy.IsValid(null!).Value);
        Assert.False(strategy.IsValid("").Value);
        Assert.False(strategy.IsValid("   ").Value);
    }

    [Fact]
    public void IsValid_ReturnsFalse_WhenKeyIsNotFound()
    {
        var strategy = CreateStrategy(new[] { "abc", "xyz" });

        var result = strategy.IsValid("not-in-list");

        Assert.False(result.Value);
    }

    [Fact]
    public void IsValid_ReturnsTrue_WhenKeyExists()
    {
        var strategy = CreateStrategy(new[] { "key123" });

        var result = strategy.IsValid("key123");

        Assert.True(result.Value);
    }

    [Fact]
    public void ValidateAndGetInfo_ReturnsFailure_WhenApiKeyIsNullOrWhitespace()
    {
        var strategy = CreateStrategy();

        var result = strategy.ValidateAndGetInfo("  ");

        Assert.False(result.IsSuccess);
        Assert.Equal(StandardErrorType.Validation, result.Error);
    }

    [Fact]
    public void ValidateAndGetInfo_ReturnsFailure_WhenBase64IsInvalid()
    {
        var strategy = CreateStrategy();

        var result = strategy.ValidateAndGetInfo("not-base64");

        Assert.False(result.IsSuccess);
        Assert.Equal(StandardErrorType.Validation, result.Error);
    }

    [Fact]
    public void ValidateAndGetInfo_ReturnsFailure_WhenDeserializedIsNull()
    {
        var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes("null"));

        var strategy = CreateStrategy();

        var result = strategy.ValidateAndGetInfo(base64);

        Assert.False(result.IsSuccess);
        Assert.Equal(StandardErrorType.Validation, result.Error);
    }

    [Fact]
    public void ValidateAndGetInfo_ReturnsSuccess_WhenApiKeyIsValidJson()
    {
        var info = new ApiKeyInfo
        {
            Key = "abc",
            Owner = "John",
            Roles = new[] { "admin" },
            Scopes = new[] { "read" }
        };

        var json = JsonSerializer.Serialize(info);
        var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));

        var strategy = CreateStrategy(new[] { base64 });

        var result = strategy.ValidateAndGetInfo(base64);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(info.Key, result.Value!.Key);
        Assert.Equal(info.Owner, result.Value.Owner);
        Assert.Equal(info.Roles, result.Value.Roles);
        Assert.Equal(info.Scopes, result.Value.Scopes);
    }

    [Fact]
    public void ValidateAndGetInfo_ReturnsFailure_WhenIsNotValidJson()
    {
        var nullJson = "null";
        var base64NullJson = Convert.ToBase64String(Encoding.UTF8.GetBytes(nullJson));

        var strategy = CreateStrategy(new[] { base64NullJson });

        var result = strategy.ValidateAndGetInfo(base64NullJson);

        Assert.False(result.IsSuccess);
        Assert.Equal(StandardErrorType.Validation, result.Error);
    }

    [Fact]
    public void ValidateAndGetInfo_ReturnFailure_Throws()
    {
        var strategy = CreateStrategy(new[] { "{}" });

        var result = strategy.ValidateAndGetInfo("{}");

        Assert.False(result.IsSuccess);
        Assert.Equal(StandardErrorType.Validation, result.Error);
    }

    [Fact]
    public void ValidateAndGetInfo_ReturnsFailure_WhenApiKeyIsNotValid()
    {
        var strategy = CreateStrategy(new[] { "key123" });

        var result = strategy.ValidateAndGetInfo("base64");

        Assert.False(result.IsSuccess);
        Assert.Equal(StandardErrorType.Validation, result.Error);
    }

    [Fact]
    public void Name_ReturnsDefault()
    {
        var strategy = CreateStrategy();
        Assert.Equal("default", strategy.Name);
    }
}