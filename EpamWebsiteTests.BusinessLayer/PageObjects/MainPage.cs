using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;
using System.Security.Cryptography.X509Certificates;

namespace EpamWebsiteTests.BusinessLayer.PageObjects;

public class MainPage : BasePage
{
    private const string MainUrl = "https://www.epam.com/";
    private readonly By careersLink = By.LinkText("Careers");
    private readonly By insightsLink = By.LinkText("Insights");
    private readonly By aboutLink = By.LinkText("About");
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

        Wait.Until(d =>
            ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState")?.ToString() == "complete");

        Driver.Manage().Cookies.AddCookie(new Cookie(
            "OptanonAlertBoxClosed",
            DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ss.fffZ"),
            ".epam.com",
            "/",
            DateTime.UtcNow.AddYears(1)));

        Driver.Navigate().Refresh();

        Wait.Until(d =>
            ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState")?.ToString() == "complete");
    }

    public void ClickCareers()
    {
        WaitUntilClickable(careersLink).Click();
    }

    public void ClickInsights()
    {
        var link = Wait.Until(driver =>
            driver.FindElements(insightsLink)
                .FirstOrDefault(element => element.Displayed && element.Enabled));

        link.Click();
    }

    public void ClickAbout()
    {
        Driver.FindElement(aboutLink).Click();
    }

    public void ClickGlobalSearchButton()
    {
        WaitUntilClickable(globalSearchButton).Click();
    }

    public void EnterGlobalSearchKeyword(string keyword)
    {
        var searchInput = Driver.FindElement(globalSearchInput);
        searchInput.SendKeys(Keys.Control + "a");
        searchInput.SendKeys(Keys.Delete);
        searchInput.SendKeys(keyword);
    }

    public void ClickGlobalSearchSubmitButton()
    {
        WaitUntilClickable(globalSearchSubmitButton).Click();
    }

    public IReadOnlyCollection<IWebElement> GetGlobalSearchResultLinks()
    {
        return Wait.Until(d =>
        {
            var links = d.FindElements(globalSearchResultLinks)
                .Where(e => e.Displayed)
                .ToList();

            return links.Count > 0 ? links : null;
        });
    }
}
