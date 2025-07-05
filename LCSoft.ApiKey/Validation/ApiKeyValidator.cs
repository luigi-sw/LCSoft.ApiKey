using LC.ApiKey.Models;
using LCSoft.Results;
using Microsoft.Extensions.Configuration;

namespace LC.ApiKey.Validation;

internal class ApiKeyValidator : IApiKeyValidator
{
    private readonly IConfiguration _configuration;
    
    private readonly Dictionary<string, ApiKeyInfo> _keys = new()
    {
        ["12345"] = new ApiKeyInfo
        {
            Key = "12345",
            Owner = "SystemA",
            Roles = new[] { "Admin", "User" },
            Scopes = new[] { "read", "write" }
        }
    };
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

    public Results<ApiKeyInfo> ValidateAndGetInfo(string apiKey)
    {
        if(_keys.TryGetValue(apiKey, out var info))
        {
            return Results<ApiKeyInfo>.Success(info);
        }
        return Results<ApiKeyInfo>.Failure(StandardErrorType.Validation);
    }
}