using EpamWebsite.Business.Models;
using EpamWebsite.Core;
using EpamWebsite.Core.ApiClient;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using RestSharp;
using Serilog;
using Serilog.Context;
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
    private IDisposable? _scenarioLogContext;
    private static IConfigurationRoot _configurationRoot = null!;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        _configurationRoot = Configuration.BuildConfiguration(AppContext.BaseDirectory);
        Logger.InitLogger(_configurationRoot);
        Log.Information("API test run started.");
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        Log.Information("API test run finished.");
        Logger.CloseAndFlush();
    }

    [SetUp] 
    public void Setup()
    {
        _apiClient = new BaseApiClient(BaseUrl);
        var testName = TestContext.CurrentContext.Test.Name;
        _scenarioLogContext = LogContext.PushProperty("Scenario", testName);
        Log.Information("[TEST START] {TestName}", testName);
    }

    [TearDown]
    public void TearDown()
    {
        var testName = TestContext.CurrentContext.Test.Name;
        var result = TestContext.CurrentContext.Result.Outcome.Status;

        if (result == NUnit.Framework.Interfaces.TestStatus.Failed)
        {
            Log.Error("Test failed: {TestName}. Message: {Message}", testName, TestContext.CurrentContext.Result.Message);
        }
        else
        {
            Log.Information("[TEST END] {TestName} finished with status {Status}", testName, result);
        }

        _scenarioLogContext?.Dispose();
    }

    [Test]
    public async Task GetUsers_ShouldReturn200AndExpectedFields()
    {
        // Arrange
        var request = new RestRequestBuilder()
            .WithEndpoint("/users")
            .WithMethod(Method.Get)
            .Build();

        // Act
        Log.Information("Sending GET /users");
        var response = await _apiClient.ExecuteAsync(request);

        // Assert
        Log.Information("Assert: validating status and transport errors.");
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

        Log.Information("Assert: validating each user has required fields: {Fields}", string.Join(", ", requiredFields));
        Assert.That(users.All(user =>
            user.ValueKind == JsonValueKind.Object &&
            requiredFields.All(field => user.TryGetProperty(field, out _))), Is.True);
    }

    [Test]
    public async Task GetUserById_ShouldReturn200AndExpectedContentTypeHeader()
    {
        // Arrange
        var request = new RestRequestBuilder()
            .WithEndpoint("/users")
            .WithMethod(Method.Get)
            .Build();

        // Act
        Log.Information("Sending GET /users for header validation");
        var response = await _apiClient.ExecuteAsync(request);

        // Assert
        Log.Information("Assert: validating status and content-type header.");
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
        // Arrange
        var request = new RestRequestBuilder()
            .WithEndpoint("/users")
            .WithMethod(Method.Get)
            .Build();

        // Act
        Log.Information("Sending GET /users for body validation");
        var response = await _apiClient.ExecuteAsync(request);

        // Assert
        Log.Information("Assert: validating status and response body structure.");
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
        var request = new RestRequestBuilder()
            .WithEndpoint("/users")
            .WithMethod(Method.Post)
            .WithJsonBody(new { name = "Test User", username = "test.user" })
            .Build();

        // Act
        Log.Information("Sending POST /users");
        var response = await _apiClient.ExecuteAsync(request);

        // Assert
        Log.Information("Assert: validating status and created object.");
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
        var request = new RestRequestBuilder()
            .WithEndpoint("/invalidendpoint")
            .WithMethod(Method.Get)
            .Build();

        // Act
        Log.Information("Sending GET /invalidendpoint");
        var response = await _apiClient.ExecuteAsync(request);

        // Assert
        Log.Information("Assert: validating 404 and no transport-level error message.");
        Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.NotFound));
        Assert.That(string.IsNullOrWhiteSpace(response.ErrorMessage), Is.True);
    }
}