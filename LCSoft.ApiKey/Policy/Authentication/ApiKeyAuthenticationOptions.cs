using Microsoft.AspNetCore.Authentication;
using Microsoft.Net.Http.Headers;

namespace LCSoft.ApiKey.Policy.Authentication;

public class ApiKeyAuthenticationOptions : AuthenticationSchemeOptions
{
    /// <summary>
    /// Api Key Value, value to be comparared, need to be register.
    /// Default: null
    /// </summary>
    /// <value></value>
    public string? ApiKey { get; set; }

    /// <summary>
    /// Scheme name, the authentication scheme.
    /// Default: ApiKeyScheme
    /// </summary>
    /// <value></value>
    public const string Scheme = Constants.Scheme;
    /// <summary>
    /// Header name, where will be search the API Key.
    /// Default: Authorization
    /// </summary>
    /// <value></value>
    public string HeaderName { get; set; } = HeaderNames.Authorization;

    /// <summary>
    /// Default Realm for WWW_Authenticate header.
    /// Default: Application
    /// </summary>
    /// <value></value>
    public string? Realm { get; set; }

    /// <summary>
    /// Default Error message for WWW_Authenticate header.
    /// </summary>
    /// <value></value>
    public string ErrorMessage { get; set; } =
        "An API key is required to access this resource. Include it in the Authorization header as: ApiKey <your-key>";
}