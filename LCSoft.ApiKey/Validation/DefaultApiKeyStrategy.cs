using LCSoft.ApiKey.Models;
using LCSoft.Results;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text;

namespace LCSoft.ApiKey.Validation;

internal class DefaultApiKeyStrategy : IApiKeyValidationStrategy
{
    private readonly DefaultApiKeyStrategyOptions _options;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public DefaultApiKeyStrategy(IOptions<DefaultApiKeyStrategyOptions> options)
    {
        _options = options.Value;
    }

    public string Name => "default";
    public Results<bool> IsValid(string apiKey)
    {
        return !string.IsNullOrWhiteSpace(apiKey) &&
           _options.ApiKeys.Contains(apiKey);
    }

    public Results<ApiKeyInfo> ValidateAndGetInfo(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            return Results<ApiKeyInfo>.Failure(StandardErrorType.Validation);

        try
        {
            var bytes = Convert.FromBase64String(apiKey);
            var json = Encoding.UTF8.GetString(bytes);

            var info = JsonSerializer.Deserialize<ApiKeyInfo>(json, _jsonOptions);

            if (info is null)
                return Results<ApiKeyInfo>.Failure(StandardErrorType.Validation);

            return Results<ApiKeyInfo>.Success(info);
        }
        catch
        {
            return Results<ApiKeyInfo>.Failure(StandardErrorType.Validation);
        }
    }
}