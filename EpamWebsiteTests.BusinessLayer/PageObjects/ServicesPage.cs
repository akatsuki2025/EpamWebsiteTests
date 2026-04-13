using EpamWebsiteTests.BusinessLayer.PageObjects;
using OpenQA.Selenium;
using Serilog;

namespace EpamWebsite.Business.PageObjects;

public class ServicesPage : BasePage
{
    private readonly By pageTitle = By.CssSelector(".scaling-of-text-wrapper");
    private readonly By relatedExpertiseSection = By.XPath("//*[contains(., 'Our Related Expertise')]");

    public ServicesPage(IWebDriver driver) : base(driver)
    { 
    }

    public bool PageContainsTitle(string expectedTitle)
    {
        Log.Information("Checking if page contains title: {ExpectedTitle}", expectedTitle);
        var titleElement = WaitForDisplayedElementWithText(pageTitle, expectedTitle);
        Log.Debug("Title element found: {TitleElement}", titleElement);
        bool result = titleElement != null;
        Log.Information("PageContainsTitle result: {Result}", result);
        return result;
    }

    public bool IsRelatedExpertiseSectionDisplayed()
    {
        Log.Information("Checking if 'Our Related Expertise' section is displayed.");
        var section = WaitForDisplayedElement(relatedExpertiseSection);
        bool result = section != null;
        Log.Information("'Our Related Expertise' section displayed: {Result}", result);
        return result;
    }
}
