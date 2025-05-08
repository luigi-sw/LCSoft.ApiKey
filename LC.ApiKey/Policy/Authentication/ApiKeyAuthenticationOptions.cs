using Microsoft.AspNetCore.Authentication;
using Microsoft.Net.Http.Headers;

namespace LC.ApiKey.Policy.Authentication;

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
}