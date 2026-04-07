using RestSharp;
using System.Text.Json;
using System.Text.Json.Serialization;
using Xunit;
using System.Net.Http.Headers;

namespace ApiTests;

[Trait("Category", "API")]
public class JsonPlaceholderApiTests
{
    public sealed class UserDto
    {
        [JsonPropertyName("id")] public int Id { get; init; }
        [JsonPropertyName("name")] public string Name { get; init; } = string.Empty;
        [JsonPropertyName("username")] public string Username { get; init; } = string.Empty;
        [JsonPropertyName("email")] public string Email { get; init; } = string.Empty;
        [JsonPropertyName("address")] public JsonElement Address { get; init; }
        [JsonPropertyName("phone")] public string Phone { get; init; } = string.Empty;
        [JsonPropertyName("website")] public string Website { get; init; } = string.Empty;
        [JsonPropertyName("company")] public JsonElement Company { get; init; }
    }

    [Fact]
    public async Task GetUsers_ShouldReturn200AndExpectedFields()
    {
        // Arrange
        var clientOptions = new RestClientOptions("https://jsonplaceholder.typicode.com");
        var client = new RestClient(clientOptions);
        var request = new RestRequest("/users", Method.Get);

        // Act
        var response = await client.ExecuteAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.Null(response.ErrorException);
        Assert.True(string.IsNullOrWhiteSpace(response.ErrorMessage));

        Assert.False(string.IsNullOrEmpty(response.Content));

        using var usersResponseDocument = JsonDocument.Parse(response.Content!);
        var users = usersResponseDocument.RootElement.EnumerateArray().ToList();

        Assert.NotEmpty(users);

        var requiredFields = new[]
        {
            "id", "name", "username", "email", "address", "phone", "website", "company"
        };

        Assert.True(users.All(user =>
            user.ValueKind == JsonValueKind.Object &&
            requiredFields.All(field => user.TryGetProperty(field, out _))));
    }

    [Fact]
    public async Task GetUserById_ShouldReturn200AndExpectedContentTypeHeader()
    {
        // Arrange
        var clientOptions = new RestClientOptions("https://jsonplaceholder.typicode.com");
        var client = new RestClient(clientOptions);
        var request = new RestRequest("/users", Method.Get);

        // Act
        var response = await client.ExecuteAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.Null(response.ErrorException);
        Assert.True(string.IsNullOrWhiteSpace(response.ErrorMessage));

        var rawContentType =
            response.ContentHeaders?
                .FirstOrDefault(h => string.Equals(h.Name?.ToString(), "Content-Type", StringComparison.OrdinalIgnoreCase))
                ?.Value?.ToString()
            ?? response.Headers?
                .FirstOrDefault(h => string.Equals(h.Name?.ToString(), "Content-Type", StringComparison.OrdinalIgnoreCase))
                ?.Value?.ToString()
            ?? response.ContentType;

        Assert.False(string.IsNullOrWhiteSpace(rawContentType));

        var parsed = MediaTypeHeaderValue.Parse(rawContentType!);
        Assert.Equal("application/json", parsed.MediaType);
        Assert.Equal("utf-8", parsed.CharSet, ignoreCase: true);
    }

    [Fact]
    public async Task GetUsers_ShouldReturn200AndTenUniqueUsersWithRequiredFields()
    {
        // Arrange
        var clientOptions = new RestClientOptions("https://jsonplaceholder.typicode.com");
        var client = new RestClient(clientOptions);
        var request = new RestRequest("/users", Method.Get);

        // Act
        var response = await client.ExecuteAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.OK, response.StatusCode);
        Assert.Null(response.ErrorException);
        Assert.True(string.IsNullOrWhiteSpace(response.ErrorMessage));

        Assert.False(string.IsNullOrEmpty(response.Content));
        var users = JsonSerializer.Deserialize<List<UserDto>>(response.Content!);
        Assert.NotNull(users);
        Assert.Equal(10, users.Count);

        var uniqueIdsCount = users.Select(user => user.Id).Distinct().Count();
        Assert.Equal(users.Count, uniqueIdsCount);

        Assert.True(users.All(user =>
            !string.IsNullOrWhiteSpace(user.Name) &&
            !string.IsNullOrWhiteSpace(user.Username)));

        Assert.True(users.All(user =>
            user.Company.ValueKind == JsonValueKind.Object &&
            user.Company.TryGetProperty("name", out var companyName) &&
            !string.IsNullOrWhiteSpace(companyName.GetString())));
    }

    [Fact]
    public async Task CreateUser_ShouldReturn201AndCreatedUserId()
    {
        // Arrange
        var clientOptions = new RestClientOptions("https://jsonplaceholder.typicode.com");
        var client = new RestClient(clientOptions);
        var request = new RestRequest("/users", Method.Post);
            
        request.AddJsonBody(new
        {
            name = "Test User",
            username = "test.user"
        });

        // Act
        var response = await client.ExecuteAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.Created, response.StatusCode);
        Assert.Null(response.ErrorException);
        Assert.True(string.IsNullOrWhiteSpace(response.ErrorMessage));

        Assert.False(string.IsNullOrEmpty(response.Content));

        using var createdUserResponseDocument = JsonDocument.Parse(response.Content!);
        var responseObject = createdUserResponseDocument.RootElement;

        Assert.Equal(JsonValueKind.Object, responseObject.ValueKind);
        Assert.True(responseObject.TryGetProperty("id", out _));
    }

    [Fact]
    public async Task InvalidEndpoint_ShouldReturn404AndNoTransportErrors()
    {
        // Arrange
        var clientOptions = new RestClientOptions("https://jsonplaceholder.typicode.com");
        var client = new RestClient(clientOptions);
        var request = new RestRequest("/invalid-endpoint", Method.Get);

        // Act
        var response = await client.ExecuteAsync(request, TestContext.Current.CancellationToken);

        // Assert
        Assert.Equal(System.Net.HttpStatusCode.NotFound, response.StatusCode);
        Assert.True(string.IsNullOrWhiteSpace(response.ErrorMessage));
    }
}