using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace EpamWebsiteTests.BusinessLayer.PageObjects;

public abstract class BasePage
{
    protected const int DefaultTimeoutInSeconds = 10;
    protected readonly IWebDriver Driver;
    protected readonly WebDriverWait Wait;

    private readonly By acceptCookiesButton = By.Id("onetrust-accept-btn-handler");
    private readonly By cookieBanner = By.Id("onetrust-group-container");

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
                var button = driver.FindElement(acceptCookiesButton);
                return button.Displayed && button.Enabled ? button : null;
            });

            acceptCookies.Click();
            Wait.Until(driver =>
            {
                var banner = driver.FindElement(cookieBanner);
                return banner == null || !banner.Displayed;
            });
        }
        catch (WebDriverTimeoutException)
        {
            // Cookie banner not present, continue
        }
    }
}
