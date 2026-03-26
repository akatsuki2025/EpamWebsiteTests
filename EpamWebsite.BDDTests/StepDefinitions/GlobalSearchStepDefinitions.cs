using EpamWebsiteTests.BusinessLayer.PageObjects;
using OpenQA.Selenium;
using Reqnroll;
using System;

namespace EpamWebsite.BDDTests.StepDefinitions;

[Binding]
public class GlobalSearchStepDefinitions
{
    private readonly IWebDriver _driver;
    private readonly MainPage _mainPage;

    public GlobalSearchStepDefinitions(ScenarioContext scenarioContext)
    {
        _driver = (IWebDriver)scenarioContext["WebDriver"];
        _mainPage = new MainPage(_driver);
    }

    [When("I open the global search")]
    public void WhenIOpenTheGlobalSearch()
    {
        _mainPage.ClickGlobalSearchButton();
    }

    [When("I enter {string} in the global search field")]
    public void WhenIEnterInTheGlobalSearchField(string keyword)
    {
        _mainPage.EnterGlobalSearchKeyword(keyword);
    }

    [When("I submit the global search")]
    public void WhenISubmitTheGlobalSearch()
    {
        _mainPage.ClickGlobalSearchSubmitButton();
    }

    [Then("all global search result links should contain the keyword {string}")]
    public void ThenAllGlobalSearchResultLinksShouldContainTheKeyword(string keyword)
    {
        var links = _mainPage.GetGlobalSearchResultLinks();
        Assert.All(links, link => Assert.Contains(keyword, link.Text, StringComparison.OrdinalIgnoreCase));
    }
}
