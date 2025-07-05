using LC.ApiKey.Validation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace LC.ApiKey.Policy.Authentication;

internal class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
    private readonly IApiKeyValidator _apiKeyValidator;

    public ApiKeyAuthenticationHandler(IOptionsMonitor<ApiKeyAuthenticationOptions> options,
                                       ILoggerFactory logger,
                                       IApiKeyValidator apiKeyValidator,
                                       UrlEncoder encoder)
        : base(options, logger, encoder)
    {
        _apiKeyValidator = apiKeyValidator;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        string? apiKey = null;

        if (!string.IsNullOrWhiteSpace(Options.HeaderName) &&
            Request.Headers.TryGetValue(Options.HeaderName, out var customHeader) &&
            !StringValues.IsNullOrEmpty(customHeader))
        {
            apiKey = customHeader.ToString();
        }
        else if (Request.Headers.TryGetValue(HeaderNames.Authorization, out var authHeader) &&
                 AuthenticationHeaderValue.TryParse(authHeader, out AuthenticationHeaderValue? headerValue) &&
                 headerValue.Scheme.Equals(Constants.Scheme, StringComparison.OrdinalIgnoreCase))
        {
            if (headerValue.Parameter is null)
            {
                return AuthenticateResult.Fail("Missing apiKey");
            }
            else
            {
                apiKey = headerValue.Parameter;
            }
        }

        if (string.IsNullOrWhiteSpace(apiKey))
            return AuthenticateResult.Fail("Missing API Key");

        var apiKeyInfo = _apiKeyValidator.ValidateAndGetInfo(apiKey);
        if (apiKeyInfo.IsError)
            return AuthenticateResult.Fail("Invalid API Key");

        if (apiKeyInfo.Value is null)
            return AuthenticateResult.Fail("API Key not found");

        var owner = !string.IsNullOrWhiteSpace(apiKeyInfo.Value.Owner) ? apiKeyInfo.Value.Owner : "Unknown";
        var roles = apiKeyInfo.Value.Roles ?? [];
        var scopes = apiKeyInfo.Value.Scopes ?? [];

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, owner),
            new(Constants.ApiKeyName, apiKey)
        };

        if (roles?.Length > 0)
        {
            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
        }

        if (scopes?.Length > 0)
        {
            claims.AddRange(scopes.Select(scope => new Claim("scope", scope)));
        }

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        
        return AuthenticateResult.Success(ticket);
    }

    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        var realm = string.IsNullOrWhiteSpace(Options.Realm)
        ? Constants.DefaultRealm
        : Options.Realm;

        Response.Headers.WWWAuthenticate = $"ApiKey realm=\"{realm}\", charset=\"UTF-8\"";
        Response.StatusCode = StatusCodes.Status401Unauthorized;
        Response.ContentType = "application/json";

        var response = new
        {
            error = "Unauthorized",
            message = Options.ErrorMessage
        };

        await Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
