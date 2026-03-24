using OpenQA.Selenium;
using OpenQA.Selenium.Edge;

namespace EpamWebsite.Core.WebDriver.Factories;

public class EdgeDriverFactory : IBrowserFactory
{
    public WebDriverSession Create(string downloadDirectory)
    {
        Directory.CreateDirectory(downloadDirectory);

        var options = BuildOptions(downloadDirectory);
        var driver = new EdgeDriver(options);

        ConfigureDownloads(driver, downloadDirectory);

        return new WebDriverSession(driver);
    }

    private static EdgeOptions BuildOptions(string downloadDirectory)
    {
        var options = new EdgeOptions();

        options.AddUserProfilePreference("download.default_directory", downloadDirectory);
        options.AddUserProfilePreference("download.prompt_for_download", false);
        options.AddUserProfilePreference("download.directory_upgrade", true);
        options.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
        options.AddUserProfilePreference("safebrowsing.enabled", true);

        return options;
    }

    private static void ConfigureDownloads(EdgeDriver driver, string downloadDirectory)
    {
        driver.ExecuteCdpCommand(
            "Page.setDownloadBehavior",
            new Dictionary<string, object?>
            {
                ["behavior"] = "allow",
                ["downloadPath"] = downloadDirectory
            });
    }
}