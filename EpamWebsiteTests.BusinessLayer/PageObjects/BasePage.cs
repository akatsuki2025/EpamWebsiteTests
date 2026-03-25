using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System.Text.RegularExpressions;
using Serilog;

namespace EpamWebsiteTests.BusinessLayer.PageObjects;

public abstract class BasePage
{
    protected const int DefaultTimeoutInSeconds = 10;
    protected readonly IWebDriver Driver;
    protected readonly WebDriverWait Wait;

    protected BasePage(IWebDriver driver)
    {
        Driver = driver;
        Wait = new WebDriverWait(driver, TimeSpan.FromSeconds(DefaultTimeoutInSeconds));
    }

    protected IWebElement WaitUntilClickable(By locator)
    {
        return Wait.Until(ExpectedConditions.ElementToBeClickable(locator));
    }

    protected IWebElement WaitUntilVisible(By locator)
    {
        return Wait.Until(ExpectedConditions.ElementIsVisible(locator));
    }

    public static async Task<string> WaitForDownloadedFileAsync(
         string downloadDirectory,
         string expectedFileName,
         TimeSpan timeout,
         CancellationToken cancellationToken = default)
    {
        var expectedBase = Path.GetFileNameWithoutExtension(expectedFileName);

        static bool IsPartial(string path) =>
            path.EndsWith(".crdownload", StringComparison.OrdinalIgnoreCase);

        static bool IsNonEmpty(string path) =>
            File.Exists(path) && new FileInfo(path).Length > 0;

        var started = DateTime.UtcNow;

        while (DateTime.UtcNow - started < timeout)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var files = Directory.EnumerateFiles(downloadDirectory).ToList();

            var completedMatch = files.FirstOrDefault(path =>
                !IsPartial(path) &&
                Path.GetExtension(path).Equals(".pdf", StringComparison.OrdinalIgnoreCase) &&
                Path.GetFileNameWithoutExtension(path).Contains(expectedBase, StringComparison.OrdinalIgnoreCase) &&
                IsNonEmpty(path));

            if (completedMatch is not null)
            {
                return completedMatch;
            }

            await Task.Delay(250, cancellationToken);
        }

        var existing = Directory.Exists(downloadDirectory)
            ? string.Join(", ", Directory.EnumerateFiles(downloadDirectory).Select(Path.GetFileName))
            : "<directory missing>";

        throw new TimeoutException(
            $"File like '{expectedFileName}' was not downloaded within {timeout}. Found: {existing}");
    }

    protected WebDriverWait CreateWait(int seconds, int pollingMs = 150, bool ignoreStale = true)
    {
        var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(seconds))
        {
            PollingInterval = TimeSpan.FromMilliseconds(pollingMs)
        };

        if (ignoreStale)
        {
            wait.IgnoreExceptionTypes(typeof(StaleElementReferenceException));
        }

        return wait;
    }

    protected static string Normalize(string text)
    {
        Log.Debug("Normalize called for text: {Text}", text);
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var normalized = Regex.Replace(text, "\\s+", " ").Trim();
        Log.Debug("Normalized text: {Normalized}", normalized);
        return normalized;
    }
}
