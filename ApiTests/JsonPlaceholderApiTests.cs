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
    private BaseApiClient _apiClient = null!;
    private IDisposable? _scenarioLogContext;
    private static IConfigurationRoot _configurationRoot = null!;
    private static Configuration _configuration = null!;

    [OneTimeSetUp]
    public void OneTimeSetup()
    {
        _configurationRoot = Configuration.BuildConfiguration(AppContext.BaseDirectory);
        _configuration = Configuration.FromRoot(_configurationRoot);
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
        _apiClient = new BaseApiClient(_configuration.Api.BaseUrl);
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
        var response = await _apiClient.ExecuteAsync(request);

        // Assert
        Log.Information("Assert: validating status and transport errors.");
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(response.ErrorException, Is.Null);
            Assert.That(response.ErrorMessage, Is.Null.Or.WhiteSpace);
            Assert.That(response.Content, Is.Not.Null.Or.WhiteSpace);
        });

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
        var response = await _apiClient.ExecuteAsync(request);

        // Assert
        Log.Information("Assert: validating status and content-type header.");
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(response.ErrorException, Is.Null);
            Assert.That(response.ErrorMessage, Is.Null.Or.WhiteSpace);
        });

        var rawContentType = response.GetContentType();
        Assert.That(rawContentType, Is.Not.Null.Or.WhiteSpace);

        var parsed = MediaTypeHeaderValue.Parse(rawContentType!);
        Assert.Multiple(() =>
        {
            Assert.That(parsed.MediaType, Is.EqualTo("application/json"));
            Assert.That(parsed.CharSet, Is.EqualTo("utf-8").IgnoreCase);
        });
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
        var response = await _apiClient.ExecuteAsync<List<UserDto>>(request);

        // Assert
        Log.Information("Assert: validating status and response body structure.");
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.OK));
            Assert.That(response.ErrorException, Is.Null);
            Assert.That(response.ErrorMessage, Is.Null.Or.WhiteSpace);
            Assert.That(response.Content, Is.Not.Null.Or.WhiteSpace);
            Assert.That(response.Data, Is.Not.Null);
        });

        var users = response.Data!;
        Assert.Multiple(() =>
        {
            Assert.That(users, Is.Not.Null);
            Assert.That(users, Has.Count.EqualTo(10));
            Assert.That(users.Select(user => user.Id), Is.Unique);
            Assert.That(users, Has.All.Matches<UserDto>(user =>
                !string.IsNullOrWhiteSpace(user.Name) &&
                !string.IsNullOrWhiteSpace(user.Username)));
            Assert.That(users.All(user =>
                user.Company.ValueKind == JsonValueKind.Object &&
                user.Company.TryGetProperty("name", out var companyName) &&
                !string.IsNullOrWhiteSpace(companyName.GetString())), Is.True);
        });
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
        var response = await _apiClient.ExecuteAsync<CreateUserResponseDto>(request);

        // Assert
        Log.Information("Assert: validating status and created object.");
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.Created));
            Assert.That(response.ErrorException, Is.Null);
            Assert.That(response.ErrorMessage, Is.Null.Or.WhiteSpace);
            Assert.That(response.Content, Is.Not.Null.Or.WhiteSpace);
            Assert.That(response.Data, Is.Not.Null);
        });

        Assert.That(response.Data!.Id, Is.GreaterThan(0));
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
        var response = await _apiClient.ExecuteAsync(request);

        // Assert
        Log.Information("Assert: validating 404 and no transport-level error message.");
        Assert.Multiple(() =>
        {
            Assert.That(response.StatusCode, Is.EqualTo(System.Net.HttpStatusCode.NotFound));
            Assert.That(response.ErrorMessage, Is.Null.Or.WhiteSpace);
        });
    }
}