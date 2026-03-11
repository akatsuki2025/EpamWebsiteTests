using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace EpamWebsiteTests.BusinessLayer.PageObjects;

public class MainPage : BasePage
{
    private const string MainUrl = "https://www.epam.com/";
    private readonly By careersLink = By.LinkText("Careers");
    private readonly By globalSearchButton = By.ClassName("header-search__button");
    private readonly By globalSearchInput = By.Name("q");
    private readonly By globalSearchSubmitButton = By.XPath("//button[contains(@class,'custom-search-button') and .//span[contains(text(),'Find')]]");
    private readonly By globalSearchResultLinks = By.CssSelector(".search-results__item a");

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

    public void ClickGlobalSearchButton()
    {
        Driver.FindElement(globalSearchButton).Click();
    }

    public void EnterGlobalSearchKeyword(string keyword)
    {
        var searchInput = Driver.FindElement(globalSearchInput);
        searchInput.Clear();
        searchInput.SendKeys(keyword);
    }

    public void ClickGlobalSearchSubmitButton()
    {
        Driver.FindElement(globalSearchSubmitButton).Click();
    }

    public IReadOnlyCollection<IWebElement> GetGlobalSearchResultLinks(WebDriverWait wait)
    {
        return wait.Until(driver => driver.FindElements(globalSearchResultLinks));
    }
}
