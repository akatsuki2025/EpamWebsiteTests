using EpamWebsite.Core.WebDriver;
using OpenQA.Selenium;

namespace EpamWebsiteTests.TestLayer;

public abstract class UiTestBase : IDisposable
{
    private bool disposed;

    protected readonly WebDriverSession Session;
    protected readonly IWebDriver Driver;
    protected readonly string DownloadDirectory;

    protected UiTestBase()
    {
        DownloadDirectory = Path.Combine(
            Directory.GetCurrentDirectory(),
            "EpamDownloads",
            DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff"));

        Directory.CreateDirectory(DownloadDirectory);

        Session = WebDriverFactory.Create(BrowserType.Chrome, DownloadDirectory);
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
            Session.Dispose();

            if (Directory.Exists(DownloadDirectory))
            {
                Directory.Delete(DownloadDirectory, recursive: true);
            }
        }

        disposed = true;
    }
}