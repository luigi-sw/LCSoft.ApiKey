using LCSoft.ApiKey.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace LCSoft.ApiKey.Validation;

public class ApiKeyValidationStrategyFactory : IApiKeyValidationStrategyFactory
{
    private readonly IEnumerable<IApiKeyValidationStrategy> _strategies;
    private readonly ILogger<ApiKeyValidationStrategyFactory> _logger;
    private readonly ApiSettings _options;

    public ApiKeyValidationStrategyFactory(
                    IEnumerable<IApiKeyValidationStrategy> strategies,
                    IOptions<ApiSettings> options,
                    ILogger<ApiKeyValidationStrategyFactory> logger)
    {
        _strategies = strategies;
        _logger = logger;
        _options = options.Value;
    }

    public IApiKeyValidationStrategy Create(string? strategyName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(strategyName))
            {
                Log("No strategy type configured.");
                return GetDefault();
            }

            var strategy = _strategies.FirstOrDefault(s =>
                string.Equals(s.Name, strategyName, StringComparison.OrdinalIgnoreCase));

            if (strategy == null)
            {
                Log($"No strategy found for name '{strategyName}'");
                return GetDefault();
            }

            return strategy;
        }
        catch (Exception ex)
        {
            if (!_options.SuppressFallbackLogging)
                _logger.LogError(ex, "Error resolving strategy by name.Falling back to DefaultApiKeyStrategy.");
            return GetDefault();
        }
    }

    private IApiKeyValidationStrategy GetDefault()
    {
        return _strategies.First(s => s.Name.Equals("default", StringComparison.OrdinalIgnoreCase));
    }

    private void Log(string message)
    {
        if (!_options.SuppressFallbackLogging)
            _logger.LogWarning("{Message} Falling back to DefaultApiKeyStrategy.", message);
    }
}
