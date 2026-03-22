using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Xunit;
using EpamWebsiteTests.BusinessLayer.PageObjects;

namespace EpamWebsiteTests.TestLayer;

public class EpamTests : IDisposable
{
    private readonly IWebDriver driver;
    private bool disposed = false;

    public EpamTests()
    {
        driver = new ChromeDriver();
        driver.Manage().Window.Maximize();
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
        string? jobCardText = jobsPage.ExpandAndGetLastCardText();

        Assert.Contains(keyword, jobCardText ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("BLOCKCHAIN")]
    [InlineData("Cloud")]
    [InlineData("Automation")]
    public void GlobalSearchTest(string keyword)
    {
        var mainPage = new MainPage(driver);

        mainPage.Open();
        mainPage.ClickGlobalSearchButton();
        mainPage.EnterGlobalSearchKeyword(keyword);
        mainPage.ClickGlobalSearchSubmitButton();
        var links = mainPage.GetGlobalSearchResultLinks();

        Assert.All(links, link => Assert.Contains(keyword, link.Text, StringComparison.OrdinalIgnoreCase));
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposed)
        {
            if (disposing)
            {
                driver?.Quit();
                driver?.Dispose();
            }

            disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
