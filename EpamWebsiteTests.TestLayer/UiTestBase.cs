using EpamWebsite.Core.WebDriver;
using OpenQA.Selenium;
using EpamWebsite.Core;
using Serilog;

namespace EpamWebsiteTests.TestLayer;

public abstract class UiTestBase : IDisposable
{
    private bool disposed;

    protected readonly WebDriverSession Session;
    protected readonly IWebDriver Driver;
    protected readonly string DownloadDirectory;
    protected readonly string ScreenshotDirectory;

    static UiTestBase()
    {
        Logger.InitLogger();
    }

    protected UiTestBase()
    {
        var runTimestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff");

        ScreenshotDirectory = Path.Combine(
            Directory.GetCurrentDirectory(),
            "Screenshots",
            runTimestamp);

        Directory.CreateDirectory(ScreenshotDirectory);

        DownloadDirectory = Path.Combine(
            Directory.GetCurrentDirectory(),
            "EpamDownloads",
            runTimestamp);

        Directory.CreateDirectory(DownloadDirectory);

        Session = WebDriverFactory.Create(BrowserType.Chrome, DownloadDirectory);
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

        Logger.CloseAndFlush();

        disposed = true;
    }
}