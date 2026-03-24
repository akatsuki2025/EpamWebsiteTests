using EpamWebsite.Core.WebDriver;
using EpamWebsiteTests.BusinessLayer.PageObjects;
using OpenQA.Selenium;
using Xunit;

namespace EpamWebsiteTests.TestLayer;

public sealed class EpamTests : IDisposable
{
    private readonly WebDriverSession session;
    private readonly IWebDriver driver;
    private readonly string downloadDirectory;

    public EpamTests()
    {
        downloadDirectory = Path.Combine(
            Directory.GetCurrentDirectory(),
            "EpamDownloads",
            DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff"));

        Directory.CreateDirectory(downloadDirectory);

        session = WebDriverFactory.Create(BrowserType.Chrome, downloadDirectory);
        session.StartBrowser();
        driver = session.Driver;
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

    [Theory]
    [InlineData("Code-Of-Conduct_01_26.pdf")]
    public async Task DownloadFileTest(string fileName)
    {
        var cancellationToken = TestContext.Current.CancellationToken;
        var mainPage = new MainPage(driver);

        mainPage.Open();
        mainPage.ClickCodeOfEthicalConductPdf();
        var downloadedPath = await BasePage.WaitForDownloadedFileAsync(
                downloadDirectory,
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

    public void Dispose()
    {
        session.Dispose();

        if (Directory.Exists(downloadDirectory))
        {
            Directory.Delete(downloadDirectory, recursive: true);
        }
    }
}