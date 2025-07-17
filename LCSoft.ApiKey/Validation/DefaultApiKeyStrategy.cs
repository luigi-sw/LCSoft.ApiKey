using LCSoft.ApiKey.Models;
using LCSoft.Results;
using Microsoft.Extensions.Options;
using System.Text.Json;
using System.Text;
using System.Security.Claims;

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

    public Results<ClaimsPrincipal> ValidateAndGetInfo(string apiKey)
    {
        if (string.IsNullOrWhiteSpace(apiKey))
            return Results<ClaimsPrincipal>.Failure(StandardErrorType.Validation);

        if (!IsValid(apiKey).Value)
            return Results<ClaimsPrincipal>.Failure(StandardErrorType.Validation);

        try
        {
            var bytes = Convert.FromBase64String(apiKey);
            var json = Encoding.UTF8.GetString(bytes);

            var info = JsonSerializer.Deserialize<ApiKeyInfo>(json, _jsonOptions);

            if (info is null)
                return Results<ClaimsPrincipal>.Failure(StandardErrorType.Validation);


            var owner = !string.IsNullOrWhiteSpace(info.Owner) ? info.Owner : "Unknown";
            #if NET8_0_OR_GREATER
                                var roles = info.Roles ?? [];
                                var scopes = info.Scopes ?? [];
            #else
            var roles = info.Roles ?? Array.Empty<string>();
                        var scopes = info.Scopes ?? Array.Empty<string>();
            #endif

            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, owner),
                new(Constants.ApiKeyName, apiKey)
            };

            if (roles.Length > 0)
            {
                claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
            }

            if (scopes.Length > 0)
            {
                claims.AddRange(scopes.Select(scope => new Claim("scope", scope)));
            }

            var identity = new ClaimsIdentity(claims, Constants.ApiKeyName);
            var principal = new ClaimsPrincipal(identity);

            return Results<ClaimsPrincipal>.Success(principal);
        }
        catch
        {
            return Results<ClaimsPrincipal>.Failure(StandardErrorType.Validation);
        }
    }
}