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

        var checkboxLabel = WaitUntilClickable(GetWorkplaceTypeLabelBy(workType));
        new Actions(Driver).MoveToElement(checkboxLabel).Perform();
        checkboxLabel.Click();
    }

    public void ClickSearchAndWaitForResults()
    {
        WaitUntilClickable(searchButton).Click();
        WaitForResultsRefresh();
    }

    public string? ExpandAndGetLastCardText()
    {
        int? lastIndex = GetLastCardIndex();
        if (lastIndex == null)
        {
            return null;
        }

        ExpandCardAndWaitUntilOpened(lastIndex.Value);
        var fullText = WaitForStableCardText(lastIndex.Value);
        return string.IsNullOrWhiteSpace(fullText) ? null : fullText;
    }

    private void ClearLocationSelection()
    {
        WaitUntilClickable(locationInputClearButton).Click();
    }

    private void OpenLocationDropdown()
    {
        WaitUntilClickable(locationInput).Click();
        WaitUntilVisible(locationDropdownOption);
    }

    private int? GetLastCardIndex()
    {
        var cards = Driver.FindElements(jobCards);
        if (cards.Count == 0)
        {
            return null;
        }

        return cards.Count - 1;
    }

    private void WaitForResultsRefresh()
    {
        WaitForPageLoadComplete();

        Wait.Until(driver =>
        {
            var cards = driver.FindElements(jobCards);
            return cards.Count > 0 && cards.All(card => card.Displayed);
        });
    }

    private void ExpandCardAndWaitUntilOpened(int index)
    {
        Wait.Until(d =>
        {
            try
            {
                var card = GetCardByIndex(d, index);
                if (card == null)
                {
                    return false;
                }

                var classAttribute = card.GetAttribute("class") ?? string.Empty;
                var isOpened = classAttribute.Contains("opened", StringComparison.OrdinalIgnoreCase);

                if (!isOpened)
                {
                    ((IJavaScriptExecutor)d).ExecuteScript("arguments[0].click();", card);

                    classAttribute = card.GetAttribute("class") ?? string.Empty;
                    isOpened = classAttribute.Contains("opened", StringComparison.OrdinalIgnoreCase);
                }

                return isOpened;
            }
            catch (StaleElementReferenceException)
            {
                return false;
            }
        });
    }

    private string WaitForStableCardText(int index)
    {
        string? lastObservedText = null;
        int consecutiveStableReads = 0;

        var result = Wait.Until(d =>
        {
            try
            {
                var card = GetCardByIndex(d, index);
                if (card == null)
                {
                    consecutiveStableReads = 0;
                    lastObservedText = null;
                    return null;
                }

                var raw = ((IJavaScriptExecutor)d).ExecuteScript("return arguments[0].textContent;", card) as string;
                var normalized = Normalize(raw);

                if (string.IsNullOrWhiteSpace(normalized))
                {
                    consecutiveStableReads = 0;
                    lastObservedText = null;
                    return null;
                }

                if (string.Equals(lastObservedText, normalized, StringComparison.Ordinal))
                {
                    consecutiveStableReads++;
                }
                else
                {
                    lastObservedText = normalized;
                    consecutiveStableReads = 0;
                }

                return consecutiveStableReads >= 1 ? normalized : null;
            }
            catch (StaleElementReferenceException)
            {
                consecutiveStableReads = 0;
                lastObservedText = null;
                return null;
            }
        });

        if (result is null)
        {
            throw new InvalidOperationException("Failed to get stable card full text.");
        }

        return result;
    }

    private IWebElement? GetCardByIndex(ISearchContext context, int index)
    {
        var cards = context.FindElements(jobCards);
        return cards.ElementAtOrDefault(index);
    }
}