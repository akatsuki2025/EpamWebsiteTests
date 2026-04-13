using RestSharp;

namespace EpamWebsite.Core.ApiClient;

public class RestRequestBuilder
{
    private string _endpoint = "/";
    private Method _method = Method.Get;
    private object? _body;

    public RestRequestBuilder WithEndpoint(string endpoint)
    {
        _endpoint = endpoint;
        return this;
    }

    public RestRequestBuilder WithMethod(Method method)
    {
        _method = method;
        return this;
    }

    public RestRequestBuilder WithJsonBody(object body)
    {
        _body = body;
        return this;
    }

    public RestRequest Build()
    {
        var request = new RestRequest(_endpoint, _method);

        if (_body != null)
        {
            request.AddJsonBody(_body);
        }

        return request;
    }
}
