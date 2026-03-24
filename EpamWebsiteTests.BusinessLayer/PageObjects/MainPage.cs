using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace EpamWebsiteTests.BusinessLayer.PageObjects;

public class MainPage : BasePage
{
    private const string MainUrl = "https://www.epam.com/";
    private readonly By careersLink = By.LinkText("Careers");
    private readonly By insightsLink = By.LinkText("Insights");
    private readonly By globalSearchButton = By.ClassName("header-search__button");
    private readonly By globalSearchInput = By.Name("q");
    private readonly By globalSearchSubmitButton = By.XPath("//button[contains(@class,'custom-search-button') and .//span[contains(text(),'Find')]]");
    private readonly By globalSearchResultLinks = By.CssSelector(".search-results__item a");
    private readonly By footer = By.TagName("footer");
    private readonly By codeOfEthicalConductPdfLink = By.CssSelector("footer a[href*='Code-Of-Conduct'][href$='.pdf']");

    public MainPage(IWebDriver driver) : base(driver)
    {
    }

    public void OpenHomePageWithConsentCookie()
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
        WaitUntilClickable(insightsLink).Click();
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

    public void ScrollToFooter()
    {
        var footerElement = WaitUntilVisible(footer);

        ((IJavaScriptExecutor)Driver).ExecuteScript(
            "arguments[0].scrollIntoView({ block: 'start', inline: 'nearest', behavior: 'auto' });",
            footerElement);
    }

    public void ClickCodeOfEthicalConductPdf()
    {
        var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(20));

        var link = wait.Until(d =>
        {
            var element = d.FindElements(codeOfEthicalConductPdfLink)
                .FirstOrDefault(e => e.Displayed && e.Enabled);
            return element;
        }) ?? throw new NoSuchElementException("Visible Code Of Conduct PDF link was not found.");

        ((IJavaScriptExecutor)Driver).ExecuteScript(
            "arguments[0].scrollIntoView({ block: 'center', inline: 'nearest' });",
            link);

        try
        {
            wait.Until(_ => link.Displayed && link.Enabled);
            link.Click();
        }
        catch (WebDriverException)
        {
            ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].click();", link);
        }
    }
}
