using LCSoft.ApiKey.Models;
using LCSoft.ApiKey.Validation;
using LCSoft.Results;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace LCSoft.ApiKey.Tests.Validation;

public class ApiKeyValidatorTests
{
    [Fact]
    public void Constructor_CreatesStrategyFromFactory()
    {
        // Arrange
        var strategy = Substitute.For<IApiKeyValidationStrategy>();
        var factory = Substitute.For<IApiKeyValidationStrategyFactory>();
        var settings = new ApiSettings { StrategyType = "TestStrategy" };
        factory.Create("TestStrategy").Returns(strategy);

        var options = Substitute.For<IOptions<ApiSettings>>();
        options.Value.Returns(settings);

        // Act
        var validator = new ApiKeyValidator(factory, options);

        // Assert
        // A cobertura do construtor já é garantida pela chamada acima
        factory.Received(1).Create("TestStrategy");
    }

    [Fact]
    public void IsValid_DelegatesToStrategy()
    {
        // Arrange
        var strategy = Substitute.For<IApiKeyValidationStrategy>();
        var expected = Results<bool>.Success(true);
        strategy.IsValid("valid-key").Returns(expected);

        var factory = Substitute.For<IApiKeyValidationStrategyFactory>();
        factory.Create("Test").Returns(strategy);

        var options = Substitute.For<IOptions<ApiSettings>>();
        options.Value.Returns(new ApiSettings { StrategyType = "Test" });

        var validator = new ApiKeyValidator(factory, options);

        // Act
        var result = validator.IsValid("valid-key");

        // Assert
        Assert.Equal(expected, result);
        strategy.Received(1).IsValid("valid-key");
    }

    [Fact]
    public void ValidateAndGetInfo_DelegatesToStrategy()
    {
        // Arrange
        var apiKeyInfo = new ApiKeyInfo
        {
            Key = "valid-key",
            Owner = "user",
            Roles = new[] { "admin" },
            Scopes = new[] { "read", "write" }
        };

        var expected = Results<ApiKeyInfo>.Success(apiKeyInfo);

        var strategy = Substitute.For<IApiKeyValidationStrategy>();
        strategy.ValidateAndGetInfo("valid-key").Returns(expected);

        var factory = Substitute.For<IApiKeyValidationStrategyFactory>();
        factory.Create("Test").Returns(strategy);

        var options = Substitute.For<IOptions<ApiSettings>>();
        options.Value.Returns(new ApiSettings { StrategyType = "Test" });

        var validator = new ApiKeyValidator(factory, options);

        // Act
        var result = validator.ValidateAndGetInfo("valid-key");

        // Assert
        Assert.Equal(expected, result);
        strategy.Received(1).ValidateAndGetInfo("valid-key");
    }
}
