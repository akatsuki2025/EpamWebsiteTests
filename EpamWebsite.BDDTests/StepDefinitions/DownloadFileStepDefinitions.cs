using EpamWebsiteTests.BusinessLayer.PageObjects;
using OpenQA.Selenium;
using Serilog;

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
    public void ThenTheFileShouldBeDownloaded(string fileName)
    {
        var downloadDirectory = (string)_scenarioContext["DownloadDirectory"];
        Log.Information("Checking if file '{FileName}' is downloaded to directory '{Directory}'...", fileName, downloadDirectory);

        var downloadedPath = BasePage.WaitForDownloadedFile(
            downloadDirectory,
            fileName,
            TimeSpan.FromSeconds(20));
        
        Log.Debug("Downloaded file path resolved: {DownloadedPath}", downloadedPath);
        Assert.Equal(fileName, Path.GetFileName(downloadedPath), ignoreCase: true);
        Log.Information("Assertion passed: File '{FileName}' was successfully downloaded.", fileName);
    }
}
