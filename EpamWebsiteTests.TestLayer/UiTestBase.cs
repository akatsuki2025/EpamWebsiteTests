using EpamWebsite.Core;
using EpamWebsite.Core.Configurations;
using EpamWebsite.Core.WebDriver;
using Microsoft.Extensions.Configuration;
using OpenQA.Selenium;
using Serilog;
using Serilog.Context;
using Xunit;

namespace EpamWebsiteTests.TestLayer;

public abstract class UiTestBase : IDisposable
{
    private bool disposed;
    private readonly IDisposable _testLogContext;
    private readonly string _testName;

    protected readonly WebDriverSession Session;
    protected readonly IWebDriver Driver;
    protected readonly string DownloadDirectory;
    private static readonly string TestRunScreenshotDirectory;

    private static readonly IConfigurationRoot _configurationRoot = Configuration.BuildConfiguration(AppContext.BaseDirectory);
    private static readonly Configuration _configuration = Configuration.FromRoot(_configurationRoot);

    static UiTestBase()
    {
        Logger.InitLogger(_configurationRoot);
        TestRunScreenshotDirectory = TestDirectoriesHelper.GetScreenshotDirectory();

        AppDomain.CurrentDomain.ProcessExit += (_, _) =>
        {
            Logger.CloseAndFlush();

            if (Directory.Exists(TestRunScreenshotDirectory) &&
                !Directory.EnumerateFileSystemEntries(TestRunScreenshotDirectory).Any())
            {
                Directory.Delete(TestRunScreenshotDirectory);
            }
        };
    }

    protected UiTestBase()
    {
        _testName = TestContext.Current.Test?.TestDisplayName ?? GetType().Name;
        _testLogContext = LogContext.PushProperty("Scenario", GetType().Name);

        DownloadDirectory = TestDirectoriesHelper.GetDownloadDirectory();
        Directory.CreateDirectory(DownloadDirectory);

        var browserType = Enum.Parse<BrowserType>(_configuration.WebDriver.Browser, true);
        Session = WebDriverFactory.Create(browserType, DownloadDirectory);
        Session.StartBrowser();
        Driver = Session.Driver;
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
            TryCaptureScreenshotOnFailure();

            _testLogContext?.Dispose();
            Session.Dispose();

            TestDirectoriesHelper.DeleteDirectoryIfExists(DownloadDirectory);
        }

        disposed = true;
    }

    private void TryCaptureScreenshotOnFailure()
    {
        try
        {
            var testState = TestContext.Current.TestState;
            if (testState?.Result != TestResult.Failed)
            {
                return;
            }

            if (!Directory.Exists(TestRunScreenshotDirectory))
            {
                Directory.CreateDirectory(TestRunScreenshotDirectory);
            }

            Log.Error("Test failed: {TestName}", _testName);
            ScreenshotHelper.TakeScreenshot(Driver, TestRunScreenshotDirectory, _testName);
        }
        catch (Exception ex)
        {
            Log.Error(ex, "Failed to capture screenshot during dispose.");
        }
    }
}