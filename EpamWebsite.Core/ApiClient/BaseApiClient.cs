using RestSharp;

namespace EpamWebsite.Core.ApiClient;

public class BaseApiClient
{
    private readonly RestClient _client;

    public BaseApiClient(string baseUrl)
    {
        var options = new RestClientOptions(baseUrl);
        _client = new RestClient(options);
    }

    public async Task<RestResponse> GetAsync(string endpoint, CancellationToken cancellationToken = default)
    {
        var request = new RestRequest(endpoint, Method.Get);
        return await _client.ExecuteAsync(request, cancellationToken);
    }

    public async Task<RestResponse> PostAsync(string endpoint, object? body = null, CancellationToken cancellationToken = default)
    {
        var request = new RestRequest(endpoint, Method.Post);
        if (body != null)
        {
            request.AddJsonBody(body);
        }
        return await _client.ExecuteAsync(request, cancellationToken);
    }
}
