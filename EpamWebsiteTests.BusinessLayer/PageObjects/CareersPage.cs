using OpenQA.Selenium;
using Serilog;

namespace EpamWebsiteTests.BusinessLayer.PageObjects;

public class CareersPage : BasePage
{
    private readonly By startJobSearchButton = By.XPath("//a[contains(@class,'button-body')]//span[text()='Start Your Search Here']/ancestor::a");

    public CareersPage(IWebDriver driver) : base(driver)
    {
    }

    public void ClickStartJobSearch()
    {
        Log.Information("Clicking 'Start Your Search Here' button.");
        WaitUntilClickable(startJobSearchButton).Click();
    }
}
