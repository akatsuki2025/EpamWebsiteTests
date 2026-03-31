using EpamWebsiteTests.BusinessLayer.PageObjects;
using OpenQA.Selenium;
using EpamWebsite.Core;
using Serilog;

namespace EpamWebsite.BDDTests.StepDefinitions
{
    [Binding]
    public class CarouselArticleTitleStepDefinitions
    {
        private readonly IWebDriver _driver;
        private readonly MainPage _mainPage;
        private readonly InsightsPage _insightsPage;
        private readonly ArticlePage _articlePage;
        private string _carouselTitle;

        public CarouselArticleTitleStepDefinitions(ScenarioContext scenarioContext)
        {
            _driver = (IWebDriver)scenarioContext["WebDriver"];
            _mainPage = new MainPage(_driver);
            _insightsPage = new InsightsPage(_driver);
            _articlePage = new ArticlePage(_driver);
        }

        [Given("I navigate to the Insights page")]
        public void GivenINavigateToTheInsightsPage()
        {
            _mainPage.ClickInsights();
        }

        [When("I swipe the carousel {int} times")]
        public void WhenISwipeTheCarouselTimes(int swipeCount)
        {
            _insightsPage.SwipeCarouselNext(swipeCount);
            _carouselTitle = _insightsPage.GetActiveCarouselArticleTitle();
        }

        [When("I click the Read More button on the active carousel article")]
        public void WhenIClickTheReadMoreButtonOnTheActiveCarouselArticle()
        {
            _insightsPage.ClickReadMoreButton();
        }

        [Then("the article title should contain all words from the carousel title")]
        public void ThenTheArticleTitleShouldContainAllWordsFromTheCarouselTitle()
        {
            var articleTitle = _articlePage.GetArticleTitle();
            var carouselWords = TextProcessingHelper.ExtractNormalizedWords(_carouselTitle);
            var articleTitleLower = articleTitle.ToLowerInvariant();

            Assert.All(carouselWords, word => Assert.Contains(word, articleTitleLower));
        }
    }
}
