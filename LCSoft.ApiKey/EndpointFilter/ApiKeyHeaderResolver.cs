using LCSoft.ApiKey.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace LCSoft.ApiKey.EndpointFilter;

public static class ApiKeyHeaderResolver
{
    public static string Resolve(HttpContext context, IOptions<ApiSettings>? options = null, string? overrideHeaderName = null)
    {
        if (!string.IsNullOrWhiteSpace(overrideHeaderName))
            return overrideHeaderName;

        var endpointHeader = context.GetEndpoint()?
            .Metadata
            .OfType<ApiKeyHeaderMetadata>()
            .FirstOrDefault()?.HeaderName;

        if (!string.IsNullOrWhiteSpace(endpointHeader))
            return endpointHeader!;

        if (!string.IsNullOrWhiteSpace(options?.Value?.HeaderName))
            return options.Value.HeaderName;

        return Constants.ApiKeyHeaderName;
    }
}