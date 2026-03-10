using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

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

    public void AcceptCookiesIfPresent()
    {
        try
        {
            var acceptCookies = Wait.Until(driver =>
            {
                var btns = driver.FindElements(By.Id("onetrust-accept-btn-handler"));
                return btns.Count > 0 && btns[0].Displayed && btns[0].Enabled ? btns[0] : null;
            });
            acceptCookies.Click();
            Wait.Until(driver =>
            {
                var banners = driver.FindElements(By.Id("onetrust-group-container"));
                return banners.Count == 0 || !banners[0].Displayed;
            });
        }
        catch (WebDriverTimeoutException)
        {
            // Cookie banner not present, continue
        }
    }
}
