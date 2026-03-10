using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using SeleniumExtras.WaitHelpers;

namespace EpamWebsiteTests.BusinessLayer.PageObjects;

public class JobsPage : BasePage
{
    private readonly By keywordInput = By.CssSelector("input[placeholder='Search by Role or Keyword']");
    private readonly By locationInput = By.CssSelector("input[aria-label='Choose your country']");
    private readonly By locationInputClearButton = By.CssSelector(".dropdown__clear-indicator");
    private readonly By locationDropdownOption = By.CssSelector("div[class*='dropdown__option']");
    private readonly By searchButton = By.XPath("//button[.//span[contains(text(),'SEARCH')]]");
    private readonly By jobCards = By.CssSelector("div[data-testid='accordion-section-container']");

    private static readonly string[] validWorkTypes = { "Remote", "Hybrid", "Office" };

    public JobsPage(IWebDriver driver) : base(driver) { }

    private static By GetLocationOptionBy(string location) =>
        By.XPath($"//div[contains(@class,'dropdown__option') and text()=\"{location}\"]");

    private static By GetWorkplaceTypeLabelBy(string workType) =>
        By.XPath($"//input[@name='vacancy_type-{workType}']/following-sibling::label");

    public void EnterKeyword(string keyword) => Driver.FindElement(keywordInput).SendKeys(keyword);

    public void SelectLocation(string location)
    {
        ClearLocationSelection();

        if (location == "All Locations")
        { 
            return; 
        }

        OpenLocationDropdown();
        new Actions(Driver).SendKeys(location).Perform();
        Wait.Until(d => d.FindElement(GetLocationOptionBy(location))).Click();
    }

    public void SelectWorkplaceType(string workType)
    {
        if (!validWorkTypes.Contains(workType))
        {
            throw new ArgumentException($"Invalid work type: {workType}. Valid options are: {string.Join(", ", validWorkTypes)}");
        }

        var checkboxLabel = Driver.FindElement(GetWorkplaceTypeLabelBy(workType));
        new Actions(Driver).MoveToElement(checkboxLabel).Perform();
        checkboxLabel.Click();
    }

    public void ClickSearchAndWaitForResults()
    {
        Driver.FindElement(searchButton).Click();
        WaitForResultsRefresh();
    }

    public string ExpandAndGetLastCardText()
    {
        int lastIndex = GetLastCardIndex();

        ExpandCard(lastIndex, Wait);
        WaitForCardContent(lastIndex, Wait);
        return GetCardInnerText(lastIndex, Wait);
    }

    private void ClearLocationSelection()
    {
        Wait.Until(ExpectedConditions.ElementToBeClickable(locationInputClearButton)).Click();
    }

    private void OpenLocationDropdown()
    {
        Driver.FindElement(locationInput).Click();
        Wait.Until(ExpectedConditions.ElementIsVisible(locationDropdownOption));
    }

    private int GetLastCardIndex()
    {
        var cards = Driver.FindElements(jobCards);
        if (cards.Count == 0)
        {
            throw new InvalidOperationException("No job cards found.");
        }

        return cards.Count - 1;
    }

    private void WaitForResultsRefresh()
    {
        var refreshWait = new WebDriverWait(Driver, TimeSpan.FromSeconds(20));
        var firstCard = Driver.FindElements(jobCards).FirstOrDefault();

        if (firstCard != null)
        {
            refreshWait.Until(ExpectedConditions.StalenessOf(firstCard));
        }

        refreshWait.Until(driver =>
        {
            var cards = driver.FindElements(jobCards);
            return cards.Count > 0 && cards.All(card => card.Displayed);
        });
    }

    private void ExpandCard(int index, WebDriverWait wait)
    {
        wait.Until(d =>
        {
            try
            {
                var card = GetCardByIndex(d, index);
                if (card == null)
                {
                    return false;
                }

                var classAttr = card.GetAttribute("class");
                if (classAttr == null || classAttr.Contains("opened", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                ((IJavaScriptExecutor)d).ExecuteScript("arguments[0].click();", card);
                return true;
            }
            catch (StaleElementReferenceException)
            {
                return false;
            }
        });
    }

    private void WaitForCardContent(int index, WebDriverWait wait)
    {
        wait.Until(d =>
        {
            try
            {
                var card = GetCardByIndex(d, index);
                if (card == null)
                {
                    return false;
                }

                var classAttr = card.GetAttribute("class");

                return classAttr != null
                    && classAttr.Contains("opened", StringComparison.OrdinalIgnoreCase)
                    && card.Displayed;
            }
            catch (StaleElementReferenceException)
            {
                return false;
            }
        });
    }

    private string GetCardInnerText(int index, WebDriverWait wait)
    {
        var result = wait.Until(d =>
        {
            try
            {
                var cards = d.FindElements(jobCards);
                if (cards.Count <= index)
                {
                    return null;
                }

                var card = cards[index];
                return ((IJavaScriptExecutor)d).ExecuteScript("return arguments[0].innerText;", card) as string;
            }
            catch (StaleElementReferenceException)
            {
                return null;
            }
        });

        if (result is null)
        {
            throw new InvalidOperationException("Failed to get card inner text.");
        }

        return result;
    }

    private IWebElement? GetCardByIndex(ISearchContext context, int index)
    {
        var cards = context.FindElements(jobCards);
        return cards.ElementAtOrDefault(index);
    }
}