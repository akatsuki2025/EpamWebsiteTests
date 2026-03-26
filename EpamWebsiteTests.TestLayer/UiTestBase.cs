using EpamWebsite.Core;
using EpamWebsite.Core.WebDriver;
using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using Serilog;

namespace EpamWebsiteTests.TestLayer;

public abstract class UiTestBase : IDisposable
{
    private bool disposed;

    protected readonly WebDriverSession Session;
    protected readonly IWebDriver Driver;
    protected readonly string DownloadDirectory;
    protected readonly string ScreenshotDirectory;

    private static readonly IConfigurationRoot _configurationRoot =
        new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

    private static readonly Configuration _configuration = LoadConfiguration();

    private static Configuration LoadConfiguration()
    {
        var config = new Configuration();
        _configurationRoot.Bind(config);
        Logger.InitLogger(_configurationRoot);
        var _ = typeof(LoggerShutdown);
        return config;
    }

    static class LoggerShutdown
    {
        static LoggerShutdown()
        {
            AppDomain.CurrentDomain.ProcessExit += (s, e) => Logger.CloseAndFlush();
        }
    }
    protected UiTestBase()
    {
        var runTimestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff");

        ScreenshotDirectory = Path.Combine(
            Directory.GetCurrentDirectory(),
            "Screenshots",
            runTimestamp);

        Directory.CreateDirectory(ScreenshotDirectory);

        DownloadDirectory = TestDirectories.GetDownloadDirectory();
        Directory.CreateDirectory(DownloadDirectory);

        Session = WebDriverFactory.Create(
            Enum.TryParse<BrowserType>(_configuration.WebDriver.Browser, true, out var browserType)
            ? browserType
            : throw new ArgumentException($"Invalid browser type: {_configuration.WebDriver.Browser}"),
            DownloadDirectory);
        Session.StartBrowser();
        Driver = Session.Driver;
    }

    protected void RunWithLogging(Action testAction, string testName)
    {
        try
        {
            testAction();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Test failed: {TestName}", testName);
            ScreenshotHelper.TakeScreenshot(Driver, DownloadDirectory, testName);
            throw;
        }
    }

    protected async Task RunWithLoggingAsync(Func<Task> testAction, string testName)
    {
        try
        {
            await testAction();
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Test failed: {TestName}", testName);
            ScreenshotHelper.TakeScreenshot(Driver, ScreenshotDirectory, testName);
            throw;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposed)
        {
            return;
        }

        if (disposing)
        {
            Session.Dispose();

            if (Directory.Exists(DownloadDirectory))
            {
                Directory.Delete(DownloadDirectory, recursive: true);
            }

            if (Directory.Exists(ScreenshotDirectory) && !Directory.EnumerateFileSystemEntries(ScreenshotDirectory).Any())
            {
                Directory.Delete(ScreenshotDirectory, recursive: false);
            }
        }

        disposed = true;
    }
}