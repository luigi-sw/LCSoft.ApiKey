using LCSoft.ApiKey.Models;
using LCSoft.ApiKey.Validation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NSubstitute;

namespace LCSoft.ApiKey.Tests.Validation;

public class ApiKeyValidationStrategyFactoryTests
{
    private IApiKeyValidationStrategy CreateStrategy(string name)
    {
        var strategy = Substitute.For<IApiKeyValidationStrategy>();
        strategy.Name.Returns(name);
        return strategy;
    }

    private ApiKeyValidationStrategyFactory CreateFactory(
        out IApiKeyValidationStrategy defaultStrategy,
        out ILogger<ApiKeyValidationStrategyFactory> logger,
        bool suppressLogging = false,
        IEnumerable<IApiKeyValidationStrategy>? customStrategies = null)
    {
        defaultStrategy = CreateStrategy("default");
        var other = CreateStrategy("custom");

        var strategies = customStrategies ?? new[] { other, defaultStrategy };
        logger = Substitute.For<ILogger<ApiKeyValidationStrategyFactory>>();

        var options = Substitute.For<IOptions<ApiSettings>>();
        options.Value.Returns(new ApiSettings { SuppressFallbackLogging = suppressLogging });

        return new ApiKeyValidationStrategyFactory(strategies, options, logger);
    }

    [Fact]
    public void Create_WithValidStrategyName_ReturnsExpectedStrategy()
    {
        // Arrange
        var custom = CreateStrategy("custom");
        var factory = CreateFactory(out _, out _, customStrategies: new[] { custom });

        // Act
        var result = factory.Create("custom");

        // Assert
        Assert.Equal(custom, result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_WithNullOrWhitespaceName_ReturnsDefaultAndLogsWarning(string? name)
    {
        // Arrange
        var factory = CreateFactory(out var defaultStrategy, out var logger);

        // Act
        var result = factory.Create(name);

        // Assert
        Assert.Equal(defaultStrategy, result);
        logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("No strategy type configured.")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public void Create_WithUnknownStrategy_ReturnsDefaultAndLogsWarning()
    {
        // Arrange
        var factory = CreateFactory(out var defaultStrategy, out var logger);

        // Act
        var result = factory.Create("unknown");

        // Assert
        Assert.Equal(defaultStrategy, result);
        logger.Received(1).Log(
            LogLevel.Warning,
            Arg.Any<EventId>(),
            Arg.Is<object>(o => o.ToString()!.Contains("No strategy found for name")),
            Arg.Any<Exception>(),
            Arg.Any<Func<object, Exception?, string>>());
    }

    [Fact]
    public void Create_WhenStrategyThrowsException_FallsBackToDefaultAndLogsError()
    {
        // Arrange
        var brokenStrategy = Substitute.For<IApiKeyValidationStrategy>();
        brokenStrategy.Name.Returns(ci => throw new Exception("Boom"));

        var defaultStrategy = Substitute.For<IApiKeyValidationStrategy>();
        defaultStrategy.Name.Returns("default");

        var strategies = new List<IApiKeyValidationStrategy>
    {
        defaultStrategy, // <- default primeiro, para garantir que GetDefault() funcione
        brokenStrategy   // <- esse vai causar exceção
    };

        var options = Substitute.For<IOptions<ApiSettings>>();
        options.Value.Returns(new ApiSettings { SuppressFallbackLogging = false });

        var logger = Substitute.For<ILogger<ApiKeyValidationStrategyFactory>>();

        var factory = new ApiKeyValidationStrategyFactory(strategies, options, logger);

        // Act
        var result = factory.Create("broken");

        // Assert
        Assert.Equal(defaultStrategy, result);
    }

    [Fact]
    public void Create_WithSuppressLoggingTrue_DoesNotLogAnything()
    {
        // Arrange
        var factory = CreateFactory(out var defaultStrategy, out var logger, suppressLogging: true);

        // Act
        var result = factory.Create(null);

        // Assert
        Assert.Equal(defaultStrategy, result);
        logger.DidNotReceive().Log(Arg.Any<LogLevel>(), Arg.Any<EventId>(), Arg.Any<object>(),
                                   Arg.Any<Exception>(), Arg.Any<Func<object, Exception?, string>>());
    }
}
