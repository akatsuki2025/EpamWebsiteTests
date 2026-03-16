using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;

namespace EpamWebsiteTests.BusinessLayer.PageObjects;

public class AboutPage : BasePage
{
    private readonly By epamAtAGlanceSection = By.XPath("//div[@class='text']//span[contains(.,'EPAM at') and contains(.,'Glance')]");

    public AboutPage(IWebDriver driver) : base(driver)
    {
    }

    public void ScrollToEpamAtAGlance()
    {
        var glanceSection = Wait.Until(driver => driver.FindElement(epamAtAGlanceSection));

        new Actions(Driver)
            .ScrollToElement(glanceSection)
            .Perform();
    }
}
