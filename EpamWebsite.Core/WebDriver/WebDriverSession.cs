using OpenQA.Selenium;

namespace EpamWebsite.Core.WebDriver;

public sealed class WebDriverSession : IDisposable
{
    private bool isClosed;

    public IWebDriver Driver { get; }

    public WebDriverSession(IWebDriver driver)
    {
        Driver = driver ?? throw new ArgumentNullException(nameof(driver));
    }

    public void StartBrowser(bool maximize = true)
    {
        if (maximize)
        {
            Driver.Manage().Window.Maximize();
        }
    }

    public void CloseBrowser()
    {
        if (isClosed)
        {
            return;
        }

        try
        {
            Driver.Quit();
        }
        finally
        {
            Driver.Dispose();
            isClosed = true;
        }
    }

    public void Dispose()
    {
        CloseBrowser();
        GC.SuppressFinalize(this);
    }
}