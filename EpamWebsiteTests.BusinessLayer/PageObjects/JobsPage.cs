using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using SeleniumExtras.WaitHelpers;
using Serilog;

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

    public void EnterKeyword(string keyword)
    {
        Log.Information("Entering keyword: {Keyword}", keyword);
        Driver.FindElement(keywordInput).SendKeys(keyword);
    }

    public void SelectLocation(string location)
    {
        Log.Information("Selecting location: {Location}", location);
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
        Log.Information("Selecting workplace type: {WorkType}", workType);
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
        Log.Information("Clicking search button and waiting for results.");
        WaitUntilClickable(searchButton).Click();
        WaitForResultsRefresh();
    }

    public string? ExpandAndGetLastCardText()
    {
        Log.Information("Expanding and getting last job card text.");
        int? lastIndex = GetLastCardIndex();
        if (lastIndex == null)
        {
            Log.Warning("No job cards found.");
            return null;
        }

        ExpandCardAndWaitUntilOpened(lastIndex.Value);
        var fullText = WaitForStableCardText(lastIndex.Value);
        Log.Information("Retrieved text from last job card.");
        return fullText;
    }

    private void ClearLocationSelection()
    {
        Log.Debug("Clearing location selection.");
        WaitUntilClickable(locationInputClearButton).Click();
    }

    private void OpenLocationDropdown()
    {
        Log.Debug("Opening location dropdown.");
        WaitUntilClickable(locationInput).Click();
        WaitUntilVisible(locationDropdownOption);
    }

    private int? GetLastCardIndex()
    {
        Log.Debug("Getting last card index.");
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
        Log.Debug("Expanding card at index {Index}.", index);
        bool expanded = Wait.Until(d =>
        {
            try
            {
                var card = GetCardByIndex(d, index);
                if (card == null)
                {
                    Log.Debug("Card at index {Index} not found.", index);
                    return false;
                }

                var classAttribute = card.GetAttribute("class") ?? string.Empty;
                var isOpened = classAttribute.Contains("opened", StringComparison.OrdinalIgnoreCase);

                if (!isOpened)
                {
                    Log.Debug("Card not opened. Clicking to expand.");
                    ((IJavaScriptExecutor)d).ExecuteScript("arguments[0].click();", card);

                    classAttribute = card.GetAttribute("class") ?? string.Empty;
                    isOpened = classAttribute.Contains("opened", StringComparison.OrdinalIgnoreCase);
                }

                return isOpened;
            }
            catch (StaleElementReferenceException)
            {
                Log.Debug("StaleElementReferenceException caught while expanding card.");
                return false;
            }
        });

        if (expanded)
        {
            Log.Debug("Card at index {Index} successfully expanded.", index);
        }
        else
        {
            Log.Error("Failed to expand card at index {Index}.", index);
        }
    }

    private string? WaitForStableCardText(int index)
    {
        Log.Debug("Waiting for stable text in card at index {Index}.", index);
        string? lastObservedText = null;
        int consecutiveStableReads = 0;

        var result = Wait.Until(d =>
        {
            try
            {
                var card = GetCardByIndex(d, index);
                if (card == null)
                {
                    Log.Debug("Card at index {Index} not found.", index);
                    consecutiveStableReads = 0;
                    lastObservedText = null;
                    return null;
                }

                var raw = ((IJavaScriptExecutor)d).ExecuteScript("return arguments[0].textContent;", card) as string;
                var normalized = NormalizeWhitespace(raw);

                if (string.IsNullOrWhiteSpace(normalized))
                {
                    Log.Debug("Card text is empty or whitespace.");
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
                Log.Debug("StaleElementReferenceException caught while reading card text.");
                consecutiveStableReads = 0;
                lastObservedText = null;
                return null;
            }
        });

        if (result is null)
        {
            Log.Error("Failed to get stable card full text at index {Index}.", index);
        }

        return result;
    }

    private static string NormalizeWhitespace(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        return string.Join(" ", value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries));
    }

    private IWebElement? GetCardByIndex(ISearchContext context, int index)
    {
        var cards = context.FindElements(jobCards);
        return cards.ElementAtOrDefault(index);
    }
}