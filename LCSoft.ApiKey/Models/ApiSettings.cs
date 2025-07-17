namespace LCSoft.ApiKey.Models;

public class ApiSettings
{
    /// <summary>
    /// Nome do Header utilizado para envio da API Key.
    /// </summary>
    public string HeaderName { get; set; } = Constants.ApiKeyHeaderName;
    /// <summary>
    /// Tipo da estratégia concreta a ser usada para validação.
    /// </summary>
    public string? StrategyType { get; set; } = "default";

    /// <summary>
    /// Se verdadeiro, suprime logs de fallback em caso de erro ao criar a estratégia.
    /// </summary>
    public bool SuppressFallbackLogging { get; set; } = false;
}