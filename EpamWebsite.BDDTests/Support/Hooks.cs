using EpamWebsite.Core;
using EpamWebsite.Core.WebDriver;
using OpenQA.Selenium;
using Serilog;
using Serilog.Context;

namespace EpamWebsite.BDDTests.Support;

[Binding]
public class Hooks
{
    private readonly ScenarioContext _scenarioContext;
    private WebDriverSession _session;
    private IWebDriver _driver;
    private string _screenshotDirectory;
    private IDisposable? _scenarioLogContext;
    private static string _testRunScreenshotDirectory;

    public Hooks(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    [BeforeTestRun]
    public static void BeforeTestRun()
    {
        var configurationRoot = Configuration.BuildConfiguration(AppContext.BaseDirectory);
        Logger.InitLogger(configurationRoot);
        _testRunScreenshotDirectory = TestDirectoriesHelper.GetScreenshotDirectory();
    }

    [BeforeScenario]
    public void BeforeScenario()
    {
        _scenarioLogContext = LogContext.PushProperty("Scenario", _scenarioContext.ScenarioInfo.Title);

        if (_scenarioContext.ScenarioInfo.Tags.Contains("downloadFile"))
        {
            var downloadDirectory = TestDirectoriesHelper.GetDownloadDirectory();
            Directory.CreateDirectory(downloadDirectory);
            _scenarioContext["DownloadDirectory"] = downloadDirectory;
        }

        var configurationRoot = Configuration.BuildConfiguration(AppContext.BaseDirectory);
        var configuration = Configuration.FromRoot(configurationRoot);

        var browserType = Enum.Parse<BrowserType>(configuration.WebDriver.Browser, true);
        var downloadDir = _scenarioContext.TryGetValue("DownloadDirectory", out var dir) ? dir as string : null;
        _session = WebDriverFactory.Create(browserType, downloadDir);
        _session.StartBrowser();
        _driver = _session.Driver;

        _scenarioContext["WebDriver"] = _driver;
        _scenarioContext["WebDriverSession"] = _session;
    }

    [AfterStep]
    public void AfterStep()
    {
        if (_scenarioContext.TestError != null)
        {
            if (!Directory.Exists(_testRunScreenshotDirectory))
            {
                Directory.CreateDirectory(_testRunScreenshotDirectory);
            }

            Log.Error(_scenarioContext.TestError,
                "Step failed: {StepText}",
                _scenarioContext.StepContext.StepInfo.Text);

            ScreenshotHelper.TakeScreenshot(_driver, _testRunScreenshotDirectory, _scenarioContext.ScenarioInfo.Title);
        }
    }

    [AfterScenario]
    public void AfterScenario()
    {
        _session?.CloseBrowser();
        _session?.Dispose();

        if (_scenarioContext.ContainsKey("DownloadDirectory") &&
            _scenarioContext.ScenarioExecutionStatus == ScenarioExecutionStatus.OK)
        {
            var downloadDirectory = (string)_scenarioContext["DownloadDirectory"];
            TestDirectoriesHelper.DeleteDirectoryIfExists(downloadDirectory);
        }

        _scenarioLogContext?.Dispose();
    }

    [AfterTestRun]
    public static void AfterTestRun()
    {
        Log.CloseAndFlush();

        if (Directory.Exists(_testRunScreenshotDirectory) && !Directory.EnumerateFileSystemEntries(_testRunScreenshotDirectory).Any())
        {
            Directory.Delete(_testRunScreenshotDirectory);
        }
    }
}