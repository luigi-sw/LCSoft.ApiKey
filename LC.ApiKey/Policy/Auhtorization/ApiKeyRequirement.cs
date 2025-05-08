using Microsoft.AspNetCore.Authorization;

namespace LC.ApiKey.Policy.Auhtorization;

internal class ApiKeyRequirement : IAuthorizationRequirement
{
    public IReadOnlyList<string>? ApiKeys { get; set; }

    public ApiKeyRequirement(IEnumerable<string> apiKeys)
    {
        ApiKeys = apiKeys?.ToList() ?? new List<string>();
    }

    public ApiKeyRequirement() { }
}
