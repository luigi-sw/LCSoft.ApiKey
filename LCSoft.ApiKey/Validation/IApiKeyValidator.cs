using LCSoft.ApiKey.Models;
using LCSoft.Results;

namespace LCSoft.ApiKey.Validation;

public interface IApiKeyValidator
{
    bool IsValid(string apiKey);

    /// <summary>
    /// Valida e retorna os dados associados a uma API key.
    /// Retorna null se inválida.
    /// </summary>
    Results<ApiKeyInfo> ValidateAndGetInfo(string apiKey);
}
