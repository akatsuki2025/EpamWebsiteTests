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

    public async Task<RestResponse> ExecuteAsync(RestRequest request, CancellationToken cancellationToken = default)
    {
        return await _client.ExecuteAsync(request, cancellationToken);
    }
}
