using EpamWebsite.Core.WebDriver.Factories;

namespace EpamWebsite.Core.WebDriver;

public static class WebDriverFactory
{
    private static readonly Dictionary<BrowserType, IBrowserFactory> Factories = new()
    {
        { BrowserType.Chrome, new ChromeDriverFactory() },
        { BrowserType.Edge, new EdgeDriverFactory() }
    };

    public static WebDriverSession Create(BrowserType browserType, string downloadDirectory)
    {
        if (!Factories.TryGetValue(browserType, out var factory))
        {
            throw new NotSupportedException($"{browserType} is not supported.");
        }

        return factory.Create(downloadDirectory);
    }
}