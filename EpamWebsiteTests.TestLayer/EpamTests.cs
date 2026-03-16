using EpamWebsiteTests.BusinessLayer.PageObjects;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Xunit;

namespace EpamWebsiteTests.TestLayer;

public class EpamTests
{
    private static IWebDriver CreateDriver(bool headless)
    {
        var options = new ChromeOptions();
        if (headless)
        {
            options.AddArgument("--headless=new");
            options.AddArgument("--window-size=1920,1080");
        }

        var driver = new ChromeDriver(options);
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.Zero;

        if (!headless)
        {
            driver.Manage().Window.Maximize();
        }

        return driver;
    }

    [Theory]
    [InlineData(false, "Java", "All Locations", "Remote")]
    [InlineData(true, "Java", "All Locations", "Remote")]
    [InlineData(false, "Python", "Croatia", "Office")]
    [InlineData(true, "Python", "Croatia", "Office")]
    public void SearchPositionTest(bool headless, string keyword, string location, string workplaceType)
    {
        using var driver = CreateDriver(headless);

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

        var jobCardText = jobsPage.ExpandAndGetLastCardText();

        Assert.Contains(keyword, jobCardText, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData(false, "BLOCKCHAIN")]
    [InlineData(true, "BLOCKCHAIN")]
    [InlineData(false, "Cloud")]
    [InlineData(true, "Cloud")]
    [InlineData(false, "Automation")]
    [InlineData(true, "Automation")]
    public void GlobalSearchTest(bool headless, string keyword)
    {
        using var driver = CreateDriver(headless);
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

        var mainPage = new MainPage(driver);

        mainPage.Open();
        mainPage.ClickGlobalSearchButton();
        mainPage.EnterGlobalSearchKeyword(keyword);
        mainPage.ClickGlobalSearchSubmitButton();

        var links = mainPage.GetGlobalSearchResultLinks(wait);

        Assert.All(links, link => Assert.Contains(keyword, link.Text, StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData(false, "EPAM_Systems_Company_Overview.pdf")]
    [InlineData(true, "EPAM_Systems_Company_Overview.pdf")]
    public void DownloadFileTest(bool headless, string fileName)
    {
        using var driver = CreateDriver(headless);

        var mainPage = new MainPage(driver);
        var aboutPage = new AboutPage(driver);

        mainPage.Open();
        mainPage.ClickAbout();
        aboutPage.ScrollToEpamAtAGlance();

        // Cannot proceed with further implementation
    }

    [Theory]
    [InlineData(false, 2)]
    [InlineData(true, 2)]
    public void CarouselArticleTitleMatchesDetailPageTest(bool headless, int swipeCount)
    {
        using var driver = CreateDriver(headless);

        var mainPage = new MainPage(driver);
        var insightsPage = new InsightsPage(driver);
        var articlePage = new ArticlePage(driver);

        mainPage.Open();
        mainPage.ClickInsights();
        insightsPage.SwipeCarouselNext(swipeCount);
        var carouselTitle = insightsPage.GetActiveCarouselArticleTitle();
        insightsPage.ClickReadMoreButton();
        var articleTitle = articlePage.GetArticleTitle();

        Assert.Equal(carouselTitle, articleTitle, ignoreCase: true);
    }
}