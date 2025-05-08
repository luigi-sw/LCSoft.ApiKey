
namespace LC.ApiKey.Services;

internal class ApiKeyAuthenticationService : IApiKeyAuthenticationService
{
    public Task<bool> IsValidAsync(string apiKey)
    {
        //Write your validation code here
        return Task.FromResult(apiKey == "Test");
    }
}