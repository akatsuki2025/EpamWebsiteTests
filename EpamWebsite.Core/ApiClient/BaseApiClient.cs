using RestSharp;
using Serilog;

namespace EpamWebsite.Core.ApiClient;

public class BaseApiClient
{
    private readonly RestClient _client;

    public BaseApiClient(string baseUrl)
    {
        var options = new RestClientOptions(baseUrl);
        _client = new RestClient(options);
    }

    public async Task<RestResponse> ExecuteAsync(RestRequest request, CancellationToken cancellationToken = default)
    {
        var response = await _client.ExecuteAsync(request, cancellationToken);

        Log.Information(
            "HTTP {Method} {Endpoint} -> {StatusCode}",
            request.Method,
            request.Resource,
            (int)response.StatusCode);

        return response;
    }

    public async Task<RestResponse<T>> ExecuteAsync<T>(RestRequest request, CancellationToken cancellationToken = default)
        where T : notnull
    {
        var response = await _client.ExecuteAsync<T>(request, cancellationToken);

        Log.Information(
            "HTTP {Method} {Endpoint} -> {StatusCode}",
            request.Method,
            request.Resource,
            (int)response.StatusCode);

        return response;
    }
}
