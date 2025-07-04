namespace LC.ApiKey.Models;

public class ApiKeyHeaderMetadata
{
    public string HeaderName { get; }

    public ApiKeyHeaderMetadata(string headerName)
    {
        HeaderName = headerName;
    }
}