using LCSoft.ApiKey.Validation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;
using System.Net.Http.Headers;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace LCSoft.ApiKey.Policy.Authentication;

internal class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
    private readonly IApiKeyValidator _apiKeyValidator;

    #if NET6_0 || NET7_0
    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<ApiKeyAuthenticationOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        IApiKeyValidator apiKeyValidator)
        : base(options, logger, encoder, clock)
    {
        _apiKeyValidator = apiKeyValidator;
    }
    #else
    public ApiKeyAuthenticationHandler(IOptionsMonitor<ApiKeyAuthenticationOptions> options,
                                       ILoggerFactory logger,
                                       IApiKeyValidator apiKeyValidator,
                                       UrlEncoder encoder)
        : base(options, logger, encoder)
    {
        _apiKeyValidator = apiKeyValidator;
    }
    #endif


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
                 headerValue.Scheme.Equals(Constants.ApiKeyName, StringComparison.OrdinalIgnoreCase))
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
       
        var ticket = new AuthenticationTicket(apiKeyInfo.Value, Scheme.Name);
        
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
