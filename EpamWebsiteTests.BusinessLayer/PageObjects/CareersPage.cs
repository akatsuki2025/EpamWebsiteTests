using OpenQA.Selenium;

namespace EpamWebsiteTests.BusinessLayer.PageObjects;

public class CareersPage : BasePage
{
    private readonly By startJobSearchButton = By.XPath("//a[contains(@class,'button-body')]//span[text()='Start Your Search Here']/ancestor::a");

    public CareersPage(IWebDriver driver) : base(driver)
    {
    }

    public void ClickStartJobSearch()
    {
        var searchButton = Wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(startJobSearchButton));
        searchButton.Click();
    }
}
