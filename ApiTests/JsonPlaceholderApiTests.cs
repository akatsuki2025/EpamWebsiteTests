using RestSharp;
using System.Text.Json;
using System.Text.Json.Serialization;
using NUnit.Framework;
using System.Net.Http.Headers;

[assembly: LevelOfParallelism(4)]
namespace ApiTests;

[Parallelizable(ParallelScope.All)]
[Category("API")]
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

    [Test]
    public async Task GetUsers_ShouldReturn200AndExpectedFields()
    {
        // Arrange
        var clientOptions = new RestClientOptions("https://jsonplaceholder.typicode.com");
        var client = new RestClient(clientOptions);
        var request = new RestRequest("/users", Method.Get);

        // Act
        var response = await client.ExecuteAsync(request);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
        Assert.That(response.ErrorException, Is.Null);
        Assert.That(string.IsNullOrWhiteSpace(response.ErrorMessage), Is.True);

        Assert.That(string.IsNullOrEmpty(response.Content), Is.False);

        using var usersResponseDocument = JsonDocument.Parse(response.Content!);
        var users = usersResponseDocument.RootElement.EnumerateArray().ToList();

        Assert.That(users, Is.Not.Empty);

        var requiredFields = new[]
        {
            "id", "name", "username", "email", "address", "phone", "website", "company"
        };

        Assert.That(users.All(user =>
            user.ValueKind == JsonValueKind.Object &&
            requiredFields.All(field => user.TryGetProperty(field, out _))), Is.True);
    }

    [Test]
    public async Task GetUserById_ShouldReturn200AndExpectedContentTypeHeader()
    {
        // Arrange
        var clientOptions = new RestClientOptions("https://jsonplaceholder.typicode.com");
        var client = new RestClient(clientOptions);
        var request = new RestRequest("/users", Method.Get);

        // Act
        var response = await client.ExecuteAsync(request);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
        Assert.That(response.ErrorException, Is.Null);
        Assert.That(string.IsNullOrWhiteSpace(response.ErrorMessage), Is.True);

        var rawContentType =
            response.ContentHeaders?
                .FirstOrDefault(h => string.Equals(h.Name?.ToString(), "Content-Type", StringComparison.OrdinalIgnoreCase))
                ?.Value?.ToString()
            ?? response.Headers?
                .FirstOrDefault(h => string.Equals(h.Name?.ToString(), "Content-Type", StringComparison.OrdinalIgnoreCase))
                ?.Value?.ToString()
            ?? response.ContentType;

        Assert.That(string.IsNullOrWhiteSpace(rawContentType), Is.False);

        var parsed = MediaTypeHeaderValue.Parse(rawContentType!);
        Assert.That(parsed.MediaType, Is.EqualTo("application/json"));
        Assert.That(parsed.CharSet, Is.EqualTo("utf-8").IgnoreCase);
    }

    [Test]
    public async Task GetUsers_ShouldReturn200AndTenUniqueUsersWithRequiredFields()
    {
        // Arrange
        var clientOptions = new RestClientOptions("https://jsonplaceholder.typicode.com");
        var client = new RestClient(clientOptions);
        var request = new RestRequest("/users", Method.Get);

        // Act
        var response = await client.ExecuteAsync(request);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
        Assert.That(response.ErrorException, Is.Null);
        Assert.That(string.IsNullOrWhiteSpace(response.ErrorMessage), Is.True);

        Assert.That(string.IsNullOrEmpty(response.Content), Is.False);
        var users = JsonSerializer.Deserialize<List<UserDto>>(response.Content!);
        Assert.That(users, Is.Not.Null);
        Assert.That(users!.Count, Is.EqualTo(10));

        var uniqueIdsCount = users.Select(user => user.Id).Distinct().Count();
        Assert.That(users.Count, Is.EqualTo(uniqueIdsCount));

        Assert.That(users.All(user =>
            !string.IsNullOrWhiteSpace(user.Name) &&
            !string.IsNullOrWhiteSpace(user.Username)), Is.True);

        Assert.That(users.All(user =>
            user.Company.ValueKind == JsonValueKind.Object &&
            user.Company.TryGetProperty("name", out var companyName) &&
            !string.IsNullOrWhiteSpace(companyName.GetString())), Is.True);
    }

    [Test]
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
        var response = await client.ExecuteAsync(request);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
        Assert.That(response.ErrorException, Is.Null);
        Assert.That(string.IsNullOrWhiteSpace(response.ErrorMessage), Is.True);

        Assert.That(string.IsNullOrEmpty(response.Content), Is.False);

        using var createdUserResponseDocument = JsonDocument.Parse(response.Content!);
        var responseObject = createdUserResponseDocument.RootElement;

        Assert.That(responseObject.ValueKind, Is.EqualTo(JsonValueKind.Object));
        Assert.That(responseObject.TryGetProperty("id", out _), Is.True);
    }

    [Test]
    public async Task InvalidEndpoint_ShouldReturn404AndNoTransportErrors()
    {
        // Arrange
        var clientOptions = new RestClientOptions("https://jsonplaceholder.typicode.com");
        var client = new RestClient(clientOptions);
        var request = new RestRequest("/invalid-endpoint", Method.Get);

        // Act
        var response = await client.ExecuteAsync(request);

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.NotFound));
        Assert.That(string.IsNullOrWhiteSpace(response.ErrorMessage), Is.True);
    }
}