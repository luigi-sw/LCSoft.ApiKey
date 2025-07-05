namespace LC.ApiKey.Models;

public class ApiKeyHeaderMetadata(string headerName)
{
    public string HeaderName { get; } = headerName;
}