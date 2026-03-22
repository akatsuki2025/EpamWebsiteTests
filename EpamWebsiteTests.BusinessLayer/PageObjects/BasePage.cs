using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

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
}
