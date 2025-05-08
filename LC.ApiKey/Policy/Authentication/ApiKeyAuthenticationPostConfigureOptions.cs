using Microsoft.Extensions.Options;

namespace LC.ApiKey.Policy.Authentication;

internal class ApiKeyAuthenticationPostConfigureOptions : IPostConfigureOptions<ApiKeyAuthenticationOptions>
{
    public void PostConfigure(string? name, ApiKeyAuthenticationOptions options) { }
};
