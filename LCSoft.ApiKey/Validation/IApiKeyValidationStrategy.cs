using LCSoft.ApiKey.Models;
using LCSoft.Results;

namespace LCSoft.ApiKey.Validation;

public interface IApiKeyValidationStrategy
{
    /// <summary>
    /// Name of the strategy, used to identify the strategy.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Validate if the apiKey is valid.
    /// </summary>
    /// <param name="apiKey"></param>
    /// <returns>Results<bool></returns>
    Results<bool> IsValid(string apiKey);

    /// <summary>
    /// Validate the apiKey and return its information if valid.
    /// </summary>
    /// <param name="apiKey"></param>
    /// <returns>Results<ApiKeyInfo></returns>
    Results<ApiKeyInfo> ValidateAndGetInfo(string apiKey);
}
