using EpamWebsiteTests.BusinessLayer.PageObjects;
using OpenQA.Selenium;

namespace EpamWebsite.BDDTests.StepDefinitions;

[Binding]
public class SearchPositionStepDefinitions
{
    private readonly IWebDriver _driver;
    private readonly MainPage _mainPage;
    private readonly CareersPage _careersPage;
    private readonly JobsPage _jobsPage;

    public SearchPositionStepDefinitions(ScenarioContext scenarioContext)
    {
        _driver = (IWebDriver)scenarioContext["WebDriver"];
        _mainPage = new MainPage(_driver);
        _careersPage = new CareersPage(_driver);
        _jobsPage = new JobsPage(_driver);
    }

    [Given("I am on the EPAM main page")]
    public void GivenIAmOnTheEpamMainPage()
    {
        _mainPage.OpenHomePageWithConsentCookie();
    }

    [Given("I navigate to the Careers page")]
    public void GivenINavigateToTheCareersPage()
    {
        _mainPage.ClickCareers();
    }

    [Given("I start a job search")]
    public void GivenIStartAJobSearch()
    {
        _careersPage.ClickStartJobSearch();
    }

    [When(@"I enter {string} as the job keyword")]
    public void WhenIEnterAsTheJobKeyword(string keyword)
    {
        _jobsPage.EnterKeyword(keyword);
    }

    [When("I select {string} as the location")]
    public void WhenISelectAsTheLocation(string location)
    {
        _jobsPage.SelectLocation(location);
    }

    [When(@"I select {string} as the workplace type")]
    public void WhenISelectAsTheWorkplaceType(string workplaceType)
    {
        _jobsPage.SelectWorkplaceType(workplaceType);
    }

    [When("I perform the job search")]
    public void WhenIPerformTheJobSearch()
    {
        _jobsPage.ClickSearchAndWaitForResults();
    }

    [Then(@"the last job card should contain the keyword {string}")]
    public void ThenTheLastJobCardShouldContainTheKeyword(string keyword)
    {
        var lastCardText = _jobsPage.ExpandAndGetLastCardText();
        Assert.Contains(keyword, lastCardText, StringComparison.OrdinalIgnoreCase);
    }
}
