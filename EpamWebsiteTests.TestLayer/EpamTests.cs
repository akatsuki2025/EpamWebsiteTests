using EpamWebsite.Core.WebDriver;
using EpamWebsiteTests.BusinessLayer.PageObjects;
using OpenQA.Selenium;
using Xunit;

namespace EpamWebsiteTests.TestLayer;

public class EpamTests : UiTestBase
{
    [Theory]
    [InlineData("Java", "All Locations", "Remote")]
    [InlineData("Python", "Croatia", "Office")]
    public void SearchPositionTest(string keyword, string location, string workplaceType)
    {
        var mainPage = new MainPage(Driver);
        var careersPage = new CareersPage(Driver);
        var jobsPage = new JobsPage(Driver);

        mainPage.OpenHomePageWithConsentCookie();
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
        var mainPage = new MainPage(Driver);

        mainPage.OpenHomePageWithConsentCookie();
        mainPage.ClickGlobalSearchButton();
        mainPage.EnterGlobalSearchKeyword(keyword);
        mainPage.ClickGlobalSearchSubmitButton();
        var links = mainPage.GetGlobalSearchResultLinks();

        Assert.All(links, link => Assert.Contains(keyword, link.Text, StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData("Code-Of-Conduct_01_26.pdf")]
    public async Task DownloadFileTest(string fileName)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var mainPage = new MainPage(Driver);

        mainPage.OpenHomePageWithConsentCookie();
        mainPage.ClickCodeOfEthicalConductPdf();
        var downloadedPath = await BasePage.WaitForDownloadedFileAsync(
                DownloadDirectory,
                fileName,
                TimeSpan.FromSeconds(30),
                cancellationToken);

        Assert.Equal(fileName, Path.GetFileName(downloadedPath), ignoreCase: true);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(1)]
    public void CarouselArticleTitleMatchesDetailPageTest(int swipeCount)
    {
        var mainPage = new MainPage(Driver);
        var insightsPage = new InsightsPage(Driver);
        var articlePage = new ArticlePage(Driver);

        mainPage.OpenHomePageWithConsentCookie();
        mainPage.ClickInsights();
        insightsPage.SwipeCarouselNext(swipeCount);
        var carouselTitle = insightsPage.GetActiveCarouselArticleTitle();
        insightsPage.ClickReadMoreButton();
        var articleTitle = articlePage.GetArticleTitle();

        Assert.Equal(carouselTitle, articleTitle, ignoreCase: true);
    }
}