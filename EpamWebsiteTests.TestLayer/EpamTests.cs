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
    private bool disposed = false;

    public EpamTests()
    {
        driver = new ChromeDriver();
        driver.Manage().Window.Maximize();
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.Zero;
        wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));
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
    public void GlobalSearchTest(string keyword)
    {
        var mainPage = new MainPage(driver);

        mainPage.Open();
        mainPage.ClickGlobalSearchButton();
        mainPage.EnterGlobalSearchKeyword(keyword);
        mainPage.ClickGlobalSearchSubmitButton();
        var links = mainPage.GetGlobalSearchResultLinks(wait);

        Assert.All(links, link => Assert.Contains(keyword, link.Text, StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData("EPAM_Systems_Company_Overview.pdf")]
    public void DownloadFileTest(string fileName)
    { 
        var mainPage = new MainPage(driver);
        var aboutPage = new AboutPage(driver);

        mainPage.Open();
        mainPage.ClickAbout();
        aboutPage.ScrollToEpamAtAGlance();
    }

    [Fact]
    public void CarouselArticleTitleMatchesDetailPageTest()
    {
        var mainPage = new MainPage(driver);
        var insightsPage = new InsightsPage(driver);
        var articlePage = new ArticlePage(driver);

        mainPage.Open();
        mainPage.ClickInsights();
        insightsPage.SwipeCarouselNext(2);
        var carouselTitle = insightsPage.GetActiveCarouselArticleTitle();
        insightsPage.ClickReadMoreButton();
        var articleTitle = articlePage.GetArticleTitle();

        Assert.Equal(carouselTitle, articleTitle, ignoreCase: true);
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
