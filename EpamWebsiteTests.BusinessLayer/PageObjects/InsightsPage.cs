using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using System.Text.RegularExpressions;
using Serilog;

namespace EpamWebsiteTests.BusinessLayer.PageObjects;

public class InsightsPage : BasePage
{
    private static readonly Regex counterRegex = new(@"^\d{1,2}\s*/\s*\d{1,2}$",
        RegexOptions.Compiled);

    private static readonly By nextArrowBy = By.CssSelector("button.slider__right-arrow.slider-navigation-arrow");
    private static readonly By ctaBy = By.CssSelector("a.slider-cta-link[href]");
    private static readonly By counterElementBy = By.XPath(".//*[contains(normalize-space(.),'/')]");
    private static readonly By ancestorRootBy = By.XPath(
        "./ancestor::*[.//a[contains(@class,'slider-cta-link')] and .//*[contains(@class,'slick-slide') or contains(@class,'single-slide')]][1]");
    private static readonly By TitleCandidatesBy = By.XPath(".//h1 | .//h2 | .//h3 | .//*[contains(@class,'scaling-of-text-wrapper')]");
    private static readonly By ancestorSlickClonedBy = By.XPath("./ancestor::*[contains(@class,'slick-cloned')]");
    private static readonly By ancestorRichContainerBy = By.XPath(
        "./ancestor::*[.//a[contains(@class,'slider-cta-link')] and (.//h2 or .//h3 or .//*[contains(@class,'scaling-of-text-wrapper')])][1]");
    private static readonly By ancestorClickableBy = By.XPath("./ancestor::a|./ancestor::button");
    private IWebElement? cachedRoot;

    public InsightsPage(IWebDriver driver) : base(driver)
    {
    }

    public void SwipeCarouselNext(int swipeCount)
    {
        Log.Information("SwipeCarouselNext called with swipeCount={SwipeCount}", swipeCount);
        EnsureInsightsLoaded();

        for (var i = 0; i < swipeCount; i++)
        {
            Log.Information("Swiping carousel: iteration {Iteration} of {Total}", i + 1, swipeCount);
            var expected = GetNextCarouselPosition();
            ClickWithFallback(GetClickableNextArrowButton());
            WaitForCounterValue(expected.Current, expected.Total);
        }
    }

    public string GetActiveCarouselArticleTitle()
    {
        Log.Information("GetActiveCarouselArticleTitle called.");
        var cta = GetPreparedActiveCta();
        var title = GetAndValidateTitleForCta(cta);

        Log.Information("Active carousel article title resolved: {Title}", title);
        return title;
    }

    public void ClickReadMoreButton()
    {
        Log.Information("ClickReadMoreButton called.");
        var cta = GetPreparedActiveCta();
        var state = CaptureNavigationState();

        ClickWithFallback(cta);
        WaitForNavigation(state);
    }

    private IWebElement GetPreparedActiveCta()
    {
        Log.Debug("GetPreparedActiveCta called.");
        EnsureInsightsLoaded();
        WaitForCounterStable();
        return GetAndValidateActiveCtaElement();
    }

    private void WaitForCounter(Func<(int Current, int Total), bool> condition, int timeoutSeconds = 10, int pollingMs = 150)
    {
        var wait = CreateWait(timeoutSeconds, pollingMs: pollingMs);
        wait.Until(_ =>
        {
            var pos = GetCarouselPosition();
            var result = condition(pos);
            Log.Debug("WaitForCounter polling: Current={Current}, Total={Total}, ConditionMet={Result}", pos.Current, pos.Total, result);
            return result;
        });
        Log.Debug("WaitForCounter: Condition met, finished waiting.");
    }

    private void WaitForCounterValue(int expectedCurrent, int expectedTotal)
    {
        Log.Debug("WaitForCounterValue called with expectedCurrent={ExpectedCurrent}, expectedTotal={ExpectedTotal}", expectedCurrent, expectedTotal);
        WaitForCounter(pos => pos.Current == expectedCurrent && pos.Total == expectedTotal, timeoutSeconds: 10, pollingMs: 120);
    }

    private void WaitForCounterStable()
    {
        Log.Debug("WaitForCounterStable called.");
        string? previous = null;

        WaitForCounter(pos =>
        {
            var now = $"{pos.Current}/{pos.Total}";
            var isStable = string.Equals(previous, now, StringComparison.Ordinal);
            Log.Debug("WaitForCounterStable polling: previous={Previous}, now={Now}, isStable={IsStable}", previous, now, isStable);
            previous = now;
            return isStable;
        });
    }

    private (int Current, int Total) GetNextCarouselPosition()
    {
        Log.Debug("GetNextCarouselPosition called.");
        var position = GetCarouselPosition();
        var nextCurrent = position.Current == position.Total ? 1 : position.Current + 1;
        Log.Debug("Next carousel position: Current={Current}, Total={Total}", nextCurrent, position.Total);
        return (nextCurrent, position.Total);
    }

    private void ClickWithFallback(IWebElement element)
    {
        Log.Debug("ClickWithFallback called.");
        Wait.Until(_ => element.Displayed && element.Enabled);

        try
        {
            Log.Debug("Attempting element.Click().");
            element.Click();
        }
        catch (ElementClickInterceptedException)
        {
            Log.Debug("ElementClickInterceptedException caught, using Actions fallback.");
            new Actions(Driver).MoveToElement(element).Click().Perform();
        }
    }

    private (string Url, IReadOnlyCollection<string> Handles) CaptureNavigationState()
    {
        Log.Debug("CaptureNavigationState called.");
        return (Driver.Url, Driver.WindowHandles);
    }

    private void WaitForNavigation((string Url, IReadOnlyCollection<string> Handles) before)
    {
        Log.Debug("WaitForNavigation called.");
        var wait = CreateWait(20);

        wait.Until(d =>
        {
            if (d.WindowHandles.Count > before.Handles.Count)
            {
                var newHandle = d.WindowHandles.Except(before.Handles).First();
                Log.Debug("New window handle detected: {NewHandle}", newHandle);
                d.SwitchTo().Window(newHandle);
                return true;
            }

            var urlChanged = !string.Equals(d.Url, before.Url, StringComparison.OrdinalIgnoreCase);
            Log.Debug("Window handles unchanged. URL changed: {UrlChanged}", urlChanged);
            return urlChanged;
        });
    }

    private void EnsureInsightsLoaded()
    {
        Log.Debug("EnsureInsightsLoaded called.");
        Wait.Until(d => d.Url.Contains("/insights", StringComparison.OrdinalIgnoreCase));
        _ = GetAndValidateCarouselRoot();
    }

    private IWebElement GetAndValidateCarouselRoot()
    {
        Log.Debug("GetCarouselRoot called.");
        if (TryGetValidCachedRoot(out var cached))
        {
            Log.Debug("Using cached carousel root.");
            return cached;
        }

        cachedRoot = Wait.Until(driver => FindValidRootFromVisibleArrows(driver));
        if (cachedRoot == null)
        {
            Log.Debug("Featured stories carousel root not found, throwing exception.");
            throw new NoSuchElementException("Featured stories carousel root not found.");
        }
        Log.Debug("Carousel root found and cached.");
        return cachedRoot;
    }

    private bool TryGetValidCachedRoot(out IWebElement root)
    {
        Log.Debug("TryGetValidCachedRoot called.");
        root = null!;

        if (cachedRoot == null)
        {
            Log.Debug("No cachedRoot available.");
            return false;
        }

        try
        {
            if (!cachedRoot.Displayed)
            {
                Log.Debug("Cached root is not displayed.");
                return false;
            }

            root = cachedRoot;
            Log.Debug("Cached root is valid.");
            return true;
        }
        catch (StaleElementReferenceException)
        {
            Log.Debug("StaleElementReferenceException caught, clearing cachedRoot.");
            cachedRoot = null;
            return false;
        }
    }

    private static IWebElement? FindValidRootFromVisibleArrows(IWebDriver driver)
    {
        Log.Debug("FindValidRootFromVisibleArrows called.");
        var arrows = driver.FindElements(nextArrowBy).Where(a => a.Displayed);

        foreach (var arrow in arrows)
        {
            var root = arrow.FindElements(ancestorRootBy).FirstOrDefault();

            if (IsValidCarouselRoot(root))
            {
                Log.Debug("Valid carousel root found from visible arrows.");
                return root;
            }
        }

        Log.Debug("No valid carousel root found from visible arrows.");
        return null;
    }

    private static bool IsValidCarouselRoot(IWebElement? root)
    {
        Log.Debug("IsValidCarouselRoot called.");
        if (root == null || !root.Displayed)
        {
            Log.Debug("Carousel root is null or not displayed.");
            return false;
        }

        var hasCounter = root.FindElements(counterElementBy)
            .Select(e => Normalize(e.Text))
            .Any(t => counterRegex.IsMatch(t));
        Log.Debug("Carousel root has counter: {HasCounter}", hasCounter);
        return hasCounter;
    }

    private IWebElement GetClickableNextArrowButton()
    {
        Log.Debug("GetClickableNextArrowButton called.");
        var root = GetAndValidateCarouselRoot();

        var arrow = Wait.Until(_ => root.FindElements(nextArrowBy).FirstOrDefault(e => e.Displayed && e.Enabled));
        if (arrow == null)
        {
            Log.Debug("Carousel next arrow not found, throwing exception.");
            throw new NoSuchElementException("Carousel next arrow not found.");
        }

        Log.Debug("Carousel next arrow found.");
        return arrow;
    }

    private (int Current, int Total) GetCarouselPosition()
    {
        Log.Debug("GetCarouselPosition called.");
        var root = GetAndValidateCarouselRoot();

        var counterText = Wait.Until(_ =>
        {
            var candidate = root.FindElements(counterElementBy)
                .Select(e => Normalize(e.Text))
                .FirstOrDefault(t => counterRegex.IsMatch(t));

            Log.Debug("Carousel counter candidate: {Candidate}", candidate);
            return candidate;
        });

        if (string.IsNullOrWhiteSpace(counterText))
        {
            Log.Debug("Carousel counter not found, throwing exception.");
            throw new InvalidOperationException("Carousel counter not found.");
        }

        var parts = counterText.Split('/');
        if (parts.Length != 2 ||
            !int.TryParse(parts[0].Trim(), out var current) ||
            !int.TryParse(parts[1].Trim(), out var total))
        {
            Log.Debug("Unexpected carousel counter format: {CounterText}", counterText);
            throw new InvalidOperationException($"Unexpected carousel counter format: {counterText}");
        }

        Log.Debug("Carousel position: Current={Current}, Total={Total}", current, total);
        return (current, total);
    }

    private IWebElement GetAndValidateActiveCtaElement()
    {
        Log.Debug("GetActiveCtaElement called.");
        var root = GetAndValidateCarouselRoot();

        var cta = root.FindElements(ctaBy).FirstOrDefault(IsUsableCta);
        if (cta == null)
        {
            Log.Debug("No usable Read More link found in carousel, throwing exception.");
            throw new NoSuchElementException("No usable Read More link found in carousel.");
        }

        Log.Debug("Active CTA element found.");
        return cta;
    }

    private static bool IsUsableCta(IWebElement element)
    {
        Log.Debug("IsUsableCta called.");
        if (!element.Displayed || !element.Enabled)
        {
            Log.Debug("Element not displayed or not enabled.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(element.GetAttribute("href")))
        {
            Log.Debug("Element href is null or whitespace.");
            return false;
        }

        var insideCloned = element.FindElements(ancestorSlickClonedBy).Count > 0;
        Log.Debug("Element inside cloned: {InsideCloned}", insideCloned);
        return !insideCloned;
    }

    private string GetAndValidateTitleForCta(IWebElement cta)
    {
        Log.Debug("GetAndValidateTitleForCta called.");
        var scope = GetAndValidateCardContainerForCta(cta);

        var title = Wait.Until(_ =>
        {
            var candidate = GetTitleCandidates(scope).FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(candidate.Text))
            {
                Log.Debug("Title resolved: {Title}", candidate.Text);
                return candidate.Text;
            }
            return null;
        });

        if (string.IsNullOrWhiteSpace(title))
        {
            Log.Debug("Title element not found for CTA card, throwing exception.");
            throw new NoSuchElementException("Title element not found for CTA card.");
        }

        return title;
    }

    private static List<(IWebElement Element, string Text)> GetTitleCandidates(IWebElement scope)
    {
        Log.Debug("GetTitleCandidates called.");
        return scope
            .FindElements(TitleCandidatesBy)
            .Where(e => e.Displayed && IsNotInsideClickableElement(e))
            .Select(e => (Element: e, Text: Normalize(e.Text)))
            .ToList();
    }

    private static bool IsNotInsideClickableElement(IWebElement element)
    {
        return element.FindElements(ancestorClickableBy).Count == 0;
    }

    private static IWebElement GetAndValidateCardContainerForCta(IWebElement cta)
    {
        Log.Debug("GetCardContainerForCta called.");
        var richContainer = cta.FindElements(ancestorRichContainerBy)
            .FirstOrDefault();

        if (richContainer != null)
        {
            Log.Debug("Rich container found for CTA.");
            return richContainer;
        }

        Log.Debug("Rich container not found for CTA, throwing exception.");
        throw new NoSuchElementException("Rich container not found for CTA.");
    }
}