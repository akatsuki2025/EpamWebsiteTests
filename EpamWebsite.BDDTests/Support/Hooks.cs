using EpamWebsite.Core;
using EpamWebsite.Core.WebDriver;
using OpenQA.Selenium;

namespace EpamWebsite.BDDTests.Support;

[Binding]
public class Hooks
{
    private readonly ScenarioContext _scenarioContext;
    private WebDriverSession _session;
    private IWebDriver _driver;
    private string _downloadDirectory;

    public Hooks(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
    }

    [BeforeScenario]
    public void BeforeScenario()
    {
        var config = Configuration.Load(AppContext.BaseDirectory);
        var browserType = Enum.Parse<BrowserType>(config.WebDriver.Browser, true);
        _downloadDirectory = TestDirectoriesHelper.GetDownloadDirectory();

        Directory.CreateDirectory(_downloadDirectory);

        _session = WebDriverFactory.Create(browserType, _downloadDirectory);
        _session.StartBrowser();
        _driver = _session.Driver;

        _scenarioContext["WebDriver"] = _driver;
        _scenarioContext["WebDriverSession"] = _session;
        _scenarioContext["DownloadDirectory"] = _downloadDirectory;
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
}