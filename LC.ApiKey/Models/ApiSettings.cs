namespace LC.ApiKey.Models;

public class ApiSettings
{
    public int ApiKeyLifetimeMinutes { get; set; }
    public string? HeaderName { get; set; }
}