using EpamWebsiteTests.BusinessLayer.PageObjects;
using OpenQA.Selenium;

namespace EpamWebsite.BDDTests.StepDefinitions;

[Binding]
public class DownloadFileStepDefinitions
{
    private readonly ScenarioContext _scenarioContext;
    private readonly IWebDriver _driver;
    private readonly MainPage _mainPage;

    public DownloadFileStepDefinitions(ScenarioContext scenarioContext)
    {
        _scenarioContext = scenarioContext;
        _driver = (IWebDriver)scenarioContext["WebDriver"];
        _mainPage = new MainPage(_driver);
    }

    [When("I click the Code of Ethical Conduct PDF link in the footer")]
    public void WhenIClickTheCodeOfEthicalConductPDFLinkInTheFooter()
    {
        _mainPage.ClickCodeOfEthicalConductPdf();
    }

    [Then("the file {string} should be downloaded")]
    public async Task ThenTheFileShouldBeDownloaded(string fileName)
    {
        var downloadDirectory = (string)_scenarioContext["DownloadDirectory"]; 
        var cancellationToken = default(CancellationToken);
        var downloadedPath = await BasePage.WaitForDownloadedFileAsync(
            downloadDirectory,
            fileName,
            TimeSpan.FromSeconds(20),
            cancellationToken);

        Assert.Equal(fileName, Path.GetFileName(downloadedPath), ignoreCase: true);
    }
}
