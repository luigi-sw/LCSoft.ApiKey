using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace LCSoft.ApiKey.Policy.Authentication;

internal class ApiKeyAuthenticationPostConfigureOptions : IPostConfigureOptions<ApiKeyAuthenticationOptions>
{
    public void PostConfigure(string? name, ApiKeyAuthenticationOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.HeaderName))
            options.HeaderName = HeaderNames.Authorization;
    }
};
