using Microsoft.Extensions.Configuration;

namespace LC.ApiKey.Validation;

internal class ApiKeyValidator : IApiKeyValidator
{
    private readonly IConfiguration _configuration;

    public ApiKeyValidator(IConfiguration configuration)
    {
        _configuration = configuration;
    }
    public bool IsValid(string userApiKey)
    {
        if (string.IsNullOrWhiteSpace(userApiKey))
            return false;
        
        string? apiKey = _configuration.GetValue<string>(Constants.ApiKeyName);

        if (string.IsNullOrWhiteSpace(apiKey))
            return false;

        if (!apiKey.Equals(userApiKey))
        {
            return false;
        }
        
        return true;
    }
}