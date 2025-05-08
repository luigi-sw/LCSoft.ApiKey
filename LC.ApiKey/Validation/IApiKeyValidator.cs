namespace LC.ApiKey.Validation;

public interface IApiKeyValidator
{
    bool IsValid(string apiKey);
}
