using EpamWebsite.Business.PageObjects;
using EpamWebsiteTests.BusinessLayer.PageObjects;
using OpenQA.Selenium;
using Serilog;

namespace EpamWebsite.BDDTests.StepDefinitions
{
    [Binding]
    public class NavigationToServicesStepDefinitions
    {
        private readonly IWebDriver _driver;
        private readonly MainPage _mainPage;
        private readonly ServicesPage _servicesPage;

        public NavigationToServicesStepDefinitions(ScenarioContext scenarioContext)
        {
            _driver = (IWebDriver)scenarioContext["WebDriver"];
            _mainPage = new MainPage(_driver);
            _servicesPage = new ServicesPage(_driver);
        }

        [When("I hover over the Services menu item in the main navigation")]
        public void WhenIHoverOverTheServicesMenuItemInTheMainNavigation()
        {
            _mainPage.HoverOverServicesMenu();
        }

        [When("I select the {string} category from the dropdown")]
        public void WhenISelectTheCategoryFromTheDropdown(string serviceCategory)
        {
            _mainPage.SelectServiceCategory(serviceCategory);
        }

        [Then("the page contains the {string} title")]
        public void ThenThePageContainsTheTitle(string expectedTitle)
        {
            Assert.True(_servicesPage.PageContainsTitle(expectedTitle), $"The page does not contain the '{expectedTitle}' title.");
            Log.Information("Assertion passed: The page contains the title '{ExpectedTitle}'.", expectedTitle);
        }

        [Then("the Our Related Expertise section is displayed")]
        public void ThenTheOurRelatedExpertiseSectionIsDisplayed()
        {
            Assert.True(_servicesPage.IsRelatedExpertiseSectionDisplayed(), "The 'Our Related Expertise' section is not displayed.");
            Log.Information("Assertion passed: The 'Our Related Expertise' section is displayed.");
        }
    }
}
