using LC.ApiKey.Validation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Encodings.Web;

namespace LC.ApiKey.Policy.Authentication;

internal class ApiKeyAuthenticationHandler : AuthenticationHandler<ApiKeyAuthenticationOptions>
{
    private readonly string AuthorizationHeaderName = HeaderNames.Authorization;
    private const string ApiKeySchemeName = Constants.Scheme;
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
        if (!Request.Headers.TryGetValue(AuthorizationHeaderName, out var apiKeyValues))
        {
            return AuthenticateResult.Fail("Missing API Key");
        }

        if (!AuthenticationHeaderValue.TryParse(Request.Headers[AuthorizationHeaderName], out AuthenticationHeaderValue? headerValue))
        {
            return AuthenticateResult.NoResult();
        }

        if (!ApiKeySchemeName.Equals(headerValue.Scheme, StringComparison.OrdinalIgnoreCase))
        {
            return AuthenticateResult.NoResult();
        }
        if (headerValue.Parameter is null)
        {
            //Missing key
            return AuthenticateResult.Fail("Missing apiKey");
        }

        bool isValid = _apiKeyValidator.IsValid(headerValue.Parameter);
        //var providedApiKey = apiKeyValues.FirstOrDefault();

        //if (string.IsNullOrEmpty(providedApiKey) || providedApiKey != expectedApiKey)
        //{
        //    return AuthenticateResult.Fail("Invalid API Key");
        //}

        if (!isValid)
        {
            return AuthenticateResult.Fail("Invalid apiKey");
        }

        //var claims = new Claim[]
        //{
        //    new Claim(ClaimTypes.NameIdentifier, $"{apiKey.ApiKeyId}"),
        //    new Claim(ClaimTypes.Name, apiKey.Name)
        //};
        //var claims = new[] { new Claim(ClaimTypes.Name, "APIKeyUser") };

        //var identiy = new ClaimsIdentity(claims, nameof(ApiKeySchemeHandler));

        var claims = new[] { new Claim(ClaimTypes.Name, "Service") };
        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        return AuthenticateResult.Success(ticket);
    }

    protected override async Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.Headers["WWW-Authenticate"] = $"ApiKey \", charset=\"UTF-8\"";
        await base.HandleChallengeAsync(properties);

    }

    //public Task<AuthenticateResult> AuthenticateAsync()
    //    => Task.FromResult(AuthenticateResult.NoResult());

    //public Task ChallengeAsync(AuthenticationProperties? properties)
    //    => Task.CompletedTask;

    //public Task ForbidAsync(AuthenticationProperties? properties)
    //    => Task.CompletedTask;
}
