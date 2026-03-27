using EpamWebsite.Core;
using EpamWebsite.Core.WebDriver;
using Microsoft.VisualStudio.TestPlatform.ObjectModel;
using OpenQA.Selenium;
using Serilog;

namespace EpamWebsite.BDDTests.Support;

[Binding]
public class Hooks
{
    private readonly ScenarioContext _scenarioContext;
    private WebDriverSession _session;
    private IWebDriver _driver;
    private string _downloadDirectory;
    private string _screenshotDirectory;

    public Hooks(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    [BeforeScenario]
    public void BeforeScenario()
    {
        var configurationRoot = Configuration.BuildConfiguration(AppContext.BaseDirectory);
        var configuration = Configuration.FromRoot(configurationRoot);
        Logger.InitLogger(configurationRoot);
        
        _downloadDirectory = TestDirectoriesHelper.GetDownloadDirectory();
        Directory.CreateDirectory(_downloadDirectory);

        _screenshotDirectory = TestDirectoriesHelper.GetScreenshotDirectory();
        Directory.CreateDirectory(_screenshotDirectory);

        var browserType = Enum.Parse<BrowserType>(configuration.WebDriver.Browser, true);
        _session = WebDriverFactory.Create(browserType, _downloadDirectory);
        _session.StartBrowser();
        _driver = _session.Driver;

        _scenarioContext["WebDriver"] = _driver;
        _scenarioContext["WebDriverSession"] = _session;
        _scenarioContext["DownloadDirectory"] = _downloadDirectory;
    }

    [AfterStep]
    public void AfterStep()
    {
        if (_scenarioContext.TestError != null)
        {
            Log.Error(_scenarioContext.TestError,
                "Step failed: {StepText}",
                _scenarioContext.StepContext.StepInfo.Text);

            ScreenshotHelper.TakeScreenshot(_driver, _screenshotDirectory, _scenarioContext.ScenarioInfo.Title);
        }
    }

    [AfterScenario]
    public void AfterScenario()
    {
        _session?.CloseBrowser();
        _session?.Dispose();
        
        if (_scenarioContext.ScenarioExecutionStatus == ScenarioExecutionStatus.OK)
        {
            TestDirectoriesHelper.DeleteDirectoryIfExists(_downloadDirectory);
        }
    }

    [AfterTestRun]
    public static void AfterTestRun()
    {
        Log.CloseAndFlush();
    }
}