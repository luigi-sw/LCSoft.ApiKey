namespace LCSoft.ApiKey.Validation;

public interface IApiKeyValidationStrategyFactory
{
    IApiKeyValidationStrategy Create(string? strategyName);
}
