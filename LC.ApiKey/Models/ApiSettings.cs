namespace LC.ApiKey.Models;

public class ApiSettings
{
    public int ApiKeyLifetimeMinutes { get; set; } = 90;
    public string HeaderName { get; set; } = Constants.ApiKeyHeaderName;
}