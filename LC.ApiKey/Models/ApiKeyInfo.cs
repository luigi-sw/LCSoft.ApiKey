namespace LC.ApiKey.Models;

public class ApiKeyInfo
{
    public string Key { get; set; } = "";
    public string Owner { get; set; } = "Unknown";
    public string[] Roles { get; set; } = Array.Empty<string>();
    public string[] Scopes { get; set; } = Array.Empty<string>();
}