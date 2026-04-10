using EpamWebsite.Core;
using EpamWebsite.Core.Configurations;
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
    private static readonly string TestRunScreenshotDirectory;

    private static readonly IConfigurationRoot _configurationRoot = Configuration.BuildConfiguration(AppContext.BaseDirectory);
    private static readonly Configuration _configuration = Configuration.FromRoot(_configurationRoot);

    static UiTestBase()
    {
        Logger.InitLogger(_configurationRoot);
        var _ = typeof(LoggerShutdown);
        TestRunScreenshotDirectory = TestDirectoriesHelper.GetScreenshotDirectory();
    }

    static class LoggerShutdown
    {
        static LoggerShutdown()
        {
            AppDomain.CurrentDomain.ProcessExit += (s, e) => Logger.CloseAndFlush();
            if (Directory.Exists(TestRunScreenshotDirectory) && !Directory.EnumerateFileSystemEntries(TestRunScreenshotDirectory).Any())
            {
                Directory.Delete(TestRunScreenshotDirectory);
            }
        }
    }

    protected UiTestBase()
    {
        _testLogContext = LogContext.PushProperty("Scenario", GetType().Name);

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
                if (!Directory.Exists(TestRunScreenshotDirectory))
                {
                    Directory.CreateDirectory(TestRunScreenshotDirectory);
                }

                Log.Error(ex, "Test failed: {TestName}", testName);
                ScreenshotHelper.TakeScreenshot(Driver, TestRunScreenshotDirectory, testName);
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
        }

        disposed = true;
    }
}