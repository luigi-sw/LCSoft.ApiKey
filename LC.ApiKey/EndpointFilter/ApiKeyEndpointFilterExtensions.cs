using LC.ApiKey.EndpointFilter;
using LC.ApiKey.Models;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

public static class ApiKeyEndpointFilterExtensions
{
    public static RouteHandlerBuilder RequireApiKey(
        this RouteHandlerBuilder builder,
        string? headerName = null)
    {
        if (!string.IsNullOrWhiteSpace(headerName))
        {
            builder.WithMetadata(new ApiKeyHeaderMetadata(headerName));
        }

        builder.AddEndpointFilter<ApiKeyEndpointFilter>();

        return builder;
    }

    public static RouteHandlerBuilder RequireApiKeyFactory(this RouteHandlerBuilder builder, string? headerName = null)
    {
        builder.AddEndpointFilterFactory(ApiKeyEndpointFilterFactory.CreateFactory(headerName));
        return builder;
    }
}
