using EpamWebsite.Core;
using EpamWebsiteTests.BusinessLayer.PageObjects;
using Serilog;
using System.Text.RegularExpressions;
using Xunit;

namespace EpamWebsiteTests.TestLayer;

public class EpamTests : UiTestBase
{
    [Theory]
    [InlineData("Java", "All Locations", "Remote")]
    [InlineData("Python", "Croatia", "Office")]
    public void SearchPositionTest(string keyword, string location, string workplaceType)
    {
        Log.Information($"[TEST START] SearchPositionTest with keyword='{keyword}', location='{location}', workplaceType='{workplaceType}'");

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

        Log.Information("Asserting that job card contains the keyword.");

        Assert.Contains(keyword, jobCardText ?? string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    [Theory]
    [InlineData("BLOCKCHAIN")]
    [InlineData("Cloud")]
    [InlineData("Automation")]
    public void GlobalSearchTest(string keyword)
    {
        Log.Information($"[TEST START] GlobalSearchTest with keyword='{keyword}'");
        var mainPage = new MainPage(Driver);

        mainPage.OpenHomePageWithConsentCookie();
        mainPage.ClickGlobalSearchButton();
        mainPage.EnterGlobalSearchKeyword(keyword);
        mainPage.ClickGlobalSearchSubmitButton();
        var links = mainPage.GetGlobalSearchResultLinks();

        Log.Information("Asserting all links contain the keyword.");

        Assert.All(links, link => Assert.Contains(keyword, link.Text, StringComparison.OrdinalIgnoreCase));
    }

    [Theory]
    [InlineData("Code-Of-Conduct_01_26.pdf")]
    public void DownloadFileTest(string fileName)
    {
        Log.Information($"[TEST START] DownloadFileTest with fileName='{fileName}'");

        var mainPage = new MainPage(Driver);

        mainPage.OpenHomePageWithConsentCookie();
        mainPage.ClickCodeOfEthicalConductPdf();

        var downloadedPath = BasePage.WaitForDownloadedFile(
            DownloadDirectory,
            fileName,
            TimeSpan.FromSeconds(20));

        Log.Information($"Downloaded file path: {downloadedPath}");
        Log.Information("Asserting downloaded file name matches expected.");

        Assert.Equal(fileName, Path.GetFileName(downloadedPath), ignoreCase: true);
    }

    [Theory]
    [InlineData(2)]
    [InlineData(1)]
    [InlineData(0)]
    [InlineData(3)]
    public void CarouselArticleTitleMatchesDetailPageTest(int swipeCount)
    {
        Log.Information("[TEST START] CarouselArticleTitleMatchesDetailPageTest with swipeCount={SwipeCount}", swipeCount);

        var mainPage = new MainPage(Driver);
        var insightsPage = new InsightsPage(Driver);
        var articlePage = new ArticlePage(Driver);

        mainPage.OpenHomePageWithConsentCookie();
        mainPage.ClickInsights();
        insightsPage.SwipeCarouselNext(swipeCount);
        var carouselTitle = insightsPage.GetActiveCarouselArticleTitle();
        insightsPage.ClickReadMoreButton();
        var articleTitle = articlePage.GetArticleTitle();

        Log.Information("Asserting all words from carousel title appear in article title. Carousel: '{CarouselTitle}', Article: '{ArticleTitle}'", carouselTitle, articleTitle);
        var carouselWords = TextProcessingHelper.ExtractNormalizedWords(carouselTitle);
        var articleTitleLower = articleTitle.ToLowerInvariant();

        Assert.All(carouselWords, word => Assert.Contains(word, articleTitleLower));
    }
}