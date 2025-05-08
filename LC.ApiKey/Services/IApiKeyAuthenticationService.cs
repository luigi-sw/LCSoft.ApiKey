
namespace LC.ApiKey.Services;

internal interface IApiKeyAuthenticationService
{
    Task<bool> IsValidAsync(string apiKey);
}
