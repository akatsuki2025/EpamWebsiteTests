using OpenQA.Selenium.Chrome;

namespace EpamWebsite.Core.WebDriver.Factories;

public class ChromeDriverFactory : IBrowserFactory
{
    public WebDriverSession Create(string? downloadDirectory)
    {
        var options = BuildOptions(downloadDirectory);
        var driver = new ChromeDriver(options);

        ConfigureDownloads(driver, downloadDirectory);

        return new WebDriverSession(driver);
    }

    private static ChromeOptions BuildOptions(string? downloadDirectory)
    {
        var options = new ChromeOptions();

        if (!string.IsNullOrEmpty(downloadDirectory))
        {
            options.AddUserProfilePreference("download.default_directory", downloadDirectory);
            options.AddUserProfilePreference("download.prompt_for_download", false);
            options.AddUserProfilePreference("download.directory_upgrade", true);
            options.AddUserProfilePreference("plugins.always_open_pdf_externally", true);
            options.AddUserProfilePreference("safebrowsing.enabled", true);
        }

        return options;
    }

    private static void ConfigureDownloads(ChromeDriver driver, string? downloadDirectory)
    {
        if (!string.IsNullOrEmpty(downloadDirectory))
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
}