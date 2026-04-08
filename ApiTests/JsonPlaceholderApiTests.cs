using EpamWebsite.Business.Models;
using EpamWebsite.Core.ApiClient;
using NUnit.Framework;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;

[assembly: LevelOfParallelism(4)]
namespace ApiTests;

[Parallelizable(ParallelScope.All)]
[Category("API")]
public class JsonPlaceholderApiTests
{
    private const string BaseUrl = "https://jsonplaceholder.typicode.com";
    private BaseApiClient _apiClient = null!;

    [SetUp] 
    public void Setup()
    {
        _apiClient = new BaseApiClient(BaseUrl);
    }

    [Test]
    public async Task GetUsers_ShouldReturn200AndExpectedFields()
    {
        // Act
        var response = await _apiClient.GetAsync("/users");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
        Assert.That(response.ErrorException, Is.Null);
        Assert.That(string.IsNullOrWhiteSpace(response.ErrorMessage), Is.True);

        Assert.That(string.IsNullOrEmpty(response.Content), Is.False);

        using var usersResponseDocument = JsonDocument.Parse(response.Content!);
        var users = usersResponseDocument.RootElement.EnumerateArray().ToList();

        Assert.That(users, Is.Not.Empty);

        var requiredFields = typeof(UserDto)
            .GetProperties()
            .Select(p => p.GetCustomAttribute<JsonPropertyNameAttribute>()?.Name)
            .OfType<string>()
            .ToHashSet();

        Assert.That(users.All(user =>
            user.ValueKind == JsonValueKind.Object &&
            requiredFields.All(field => user.TryGetProperty(field, out _))), Is.True);
    }

    [Test]
    public async Task GetUserById_ShouldReturn200AndExpectedContentTypeHeader()
    {
        // Act
        var response = await _apiClient.GetAsync("/users");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
        Assert.That(response.ErrorException, Is.Null);
        Assert.That(string.IsNullOrWhiteSpace(response.ErrorMessage), Is.True);

        var rawContentType = response.GetContentType();
        Assert.That(string.IsNullOrWhiteSpace(rawContentType), Is.False);

        var parsed = MediaTypeHeaderValue.Parse(rawContentType!);
        Assert.That(parsed.MediaType, Is.EqualTo("application/json"));
        Assert.That(parsed.CharSet, Is.EqualTo("utf-8").IgnoreCase);
    }

    [Test]
    public async Task GetUsers_ShouldReturn200AndTenUniqueUsersWithRequiredFields()
    {
        // Act
        var response = await _apiClient.GetAsync("/users");

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
        // Act
        var response = await _apiClient.PostAsync("/users", new
        {
            name = "Test User",
            username = "test.user"
        });

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
        // Act
        var response = await _apiClient.GetAsync("/invalid-endpoint");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.NotFound));
        Assert.That(string.IsNullOrWhiteSpace(response.ErrorMessage), Is.True);
    }
}