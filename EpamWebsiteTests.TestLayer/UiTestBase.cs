using EpamWebsite.Core;
using EpamWebsite.Core.WebDriver;
using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using Serilog;
using Serilog.Context;

namespace EpamWebsiteTests.TestLayer;

public abstract class UiTestBase : IDisposable
{
    private bool disposed;
    private readonly IDisposable _testLogContext;

    protected readonly WebDriverSession Session;
    protected readonly IWebDriver Driver;
    protected readonly string DownloadDirectory;
    protected readonly string ScreenshotDirectory;

    private static readonly IConfigurationRoot _configurationRoot = Configuration.BuildConfiguration(AppContext.BaseDirectory);
    private static readonly Configuration _configuration = Configuration.FromRoot(_configurationRoot);

    static UiTestBase()
    {
        Logger.InitLogger(_configurationRoot);
        var _ = typeof(LoggerShutdown);
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
        _testLogContext = LogContext.PushProperty("Scenario", GetType().Name);

        ScreenshotDirectory = TestDirectoriesHelper.GetScreenshotDirectory();
        Directory.CreateDirectory(ScreenshotDirectory);

        DownloadDirectory = TestDirectoriesHelper.GetDownloadDirectory();
        Directory.CreateDirectory(DownloadDirectory);

        var browserType = Enum.Parse<BrowserType>(_configuration.WebDriver.Browser, true);
        Session = WebDriverFactory.Create(browserType, DownloadDirectory);
        Session.StartBrowser();
        Driver = Session.Driver;
    }

    protected void RunWithLogging(Action testAction, string testName)
    {
        using (LogContext.PushProperty("Scenario", testName))
        {
            try
            {
                testAction();
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Test failed: {TestName}", testName);
                ScreenshotHelper.TakeScreenshot(Driver, ScreenshotDirectory, testName);
                throw;
            }
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
            _testLogContext?.Dispose();
            Session.Dispose();

            TestDirectoriesHelper.DeleteDirectoryIfExists(DownloadDirectory);

            if (Directory.Exists(ScreenshotDirectory) && !Directory.EnumerateFileSystemEntries(ScreenshotDirectory).Any())
            {
                Directory.Delete(ScreenshotDirectory, recursive: false);
            }
        }

        disposed = true;
    }
}