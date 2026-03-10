using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Xunit;
using EpamWebsiteTests.BusinessLayer.PageObjects;

namespace EpamWebsiteTests.TestLayer;

public class EpamTests : IDisposable
{
    private readonly IWebDriver driver;
    private readonly WebDriverWait wait;

    public EpamTests()
    {
        driver = new ChromeDriver();
        driver.Manage().Window.Maximize();
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
        wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
    }

    public void Dispose()
    {
        driver.Quit();
    }

    [Theory]
    [InlineData("Java", "All Locations", "Remote")]
    [InlineData("Python", "Croatia", "Office")]
    public void SearchPositionTest(string keyword, string location, string workplaceType)
    {
        var mainPage = new MainPage(driver);
        var careersPage = new CareersPage(driver);
        var jobsPage = new JobsPage(driver);

        mainPage.Open();
        mainPage.ClickCareers();
        careersPage.ClickStartJobSearch();
        jobsPage.EnterKeyword(keyword);
        jobsPage.SelectLocation(location);
        jobsPage.SelectWorkplaceType(workplaceType);
        jobsPage.ClickSearchAndWaitForResults();
        string jobCardText = jobsPage.ExpandAndGetLastCardText();

        Assert.Contains(keyword, jobCardText, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("BLOCKCHAIN")]
    [InlineData("Cloud")]
    [InlineData("Automation")]
    public void GlobalSearchTest(string searchTerm)
    {
        driver.Navigate().GoToUrl("https://www.epam.com/");
        AcceptCookiesIfPresent();

        driver.FindElement(By.ClassName("header-search__button")).Click();
        var searchInput = driver.FindElement(By.TagName("input"));
        searchInput.Clear();
        searchInput.SendKeys(searchTerm);
        driver.FindElement(By.XPath("//button[contains(@class,'custom-search-button') and .//span[contains(text(),'Find')]]")).Click();

        var links = wait.Until(d => d.FindElements(By.CssSelector(".search-results__item a")));
        Assert.All(links, link => Assert.Contains(searchTerm, link.Text, StringComparison.OrdinalIgnoreCase));
    }

    private void AcceptCookiesIfPresent()
    {
        try
        {
            var acceptCookies = wait.Until(driver =>
            {
                var btns = driver.FindElements(By.Id("onetrust-accept-btn-handler"));
                return btns.Count > 0 && btns[0].Displayed && btns[0].Enabled ? btns[0] : null;
            });
            acceptCookies.Click();
            wait.Until(driver =>
            {
                var banners = driver.FindElements(By.Id("onetrust-group-container"));
                return banners.Count == 0 || !banners[0].Displayed;
            });
        }
        catch (WebDriverTimeoutException)
        {
            // Cookie banner not present, continue
        }
    }
}
