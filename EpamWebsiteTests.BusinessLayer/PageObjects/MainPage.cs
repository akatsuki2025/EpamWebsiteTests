using OpenQA.Selenium;

namespace EpamWebsiteTests.BusinessLayer.PageObjects;

public class MainPage : BasePage
{
    private const string MainUrl = "https://www.epam.com/";
    private readonly By careersLink = By.LinkText("Careers");

    public MainPage(IWebDriver driver) : base(driver)
    {
    }

    public void Open()
    {
        Driver.Navigate().GoToUrl(MainUrl);
        Driver.Manage().Cookies.AddCookie(new Cookie(
            "OptanonAlertBoxClosed",
            DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
            ".epam.com", "/", DateTime.UtcNow.AddYears(1)));

        Driver.Navigate().Refresh();
    }

    public void ClickCareers()
    {
        Driver.FindElement(careersLink).Click();
    }
}
