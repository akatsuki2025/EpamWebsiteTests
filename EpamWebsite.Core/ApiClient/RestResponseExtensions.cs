using RestSharp;

namespace EpamWebsite.Core.ApiClient;

public static class RestResponseExtensions
{
    public static string? GetContentType(this RestResponse response) =>
        response.ContentHeaders?
            .FirstOrDefault(h => string.Equals(h.Name?.ToString(), "Content-Type", StringComparison.OrdinalIgnoreCase))
            ?.Value?.ToString()
        ?? response.Headers?
            .FirstOrDefault(h => string.Equals(h.Name?.ToString(), "Content-Type", StringComparison.OrdinalIgnoreCase))
            ?.Value?.ToString()
        ?? response.ContentType;
}