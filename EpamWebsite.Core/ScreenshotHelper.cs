using OpenQA.Selenium;
using Serilog;

namespace EpamWebsite.Core
{
    public static class ScreenshotHelper
    {
        public static void TakeScreenshot(IWebDriver driver, string directory, string testName)
        {
            try
            {
                var screenshotDriver = (ITakesScreenshot)driver;
                var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                var fileName = $"{testName}_{timestamp}.png";
                var filePath = Path.Combine(directory, fileName);
                var screenshot = screenshotDriver.GetScreenshot();
                screenshot.SaveAsFile(filePath);
                Log.Information("Screenshot saved: {FilePath}", filePath);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Failed to take screenshot for test: {TestName}", testName);
            }
        }
    }
}