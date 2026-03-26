using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System.Text.RegularExpressions;

namespace EpamWebsiteTests.BusinessLayer.PageObjects;

public class InsightsPage : BasePage
{
    private static readonly Regex counterRegex = new(@"^\d{1,2}\s*/\s*\d{1,2}$", RegexOptions.Compiled);

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
        EnsureInsightsLoaded();

        for (var i = 0; i < swipeCount; i++)
        {
            var expected = GetNextCarouselPosition();
            ClickWithFallback(GetClickableNextArrowButton());
            WaitForCounterValue(expected.Current, expected.Total);
        }
    }

    public string GetActiveCarouselArticleTitle()
    {
        var cta = GetPreparedActiveCta();
        var title = GetAndValidateTitleForCta(cta);

        return title;
    }

    public void ClickReadMoreButton()
    {
        var cta = GetPreparedActiveCta();
        var state = CaptureNavigationState();

        ClickWithFallback(cta);
        WaitForNavigation(state);
    }

    private IWebElement GetPreparedActiveCta()
    {
        EnsureInsightsLoaded();
        WaitForCounterStable();
        return GetActiveCtaElement();
    }

    private void WaitForCounter(Func<(int Current, int Total), bool> condition, int timeoutSeconds = 10, int pollingMs = 150)
    {
        var wait = CreateWait(timeoutSeconds, pollingMs: pollingMs);
        wait.Until(_ =>
        {
            var pos = GetCarouselPosition();
            var result = condition(pos);
            return result;
        });
    }

    private void WaitForCounterValue(int expectedCurrent, int expectedTotal)
    {
        WaitForCounter(pos => pos.Current == expectedCurrent && pos.Total == expectedTotal, timeoutSeconds: 10, pollingMs: 120);
    }

    private void WaitForCounterStable()
    {
        string? previous = null;

        WaitForCounter(pos =>
        {
            var now = $"{pos.Current}/{pos.Total}";
            var isStable = string.Equals(previous, now, StringComparison.Ordinal);
            previous = now;
            return isStable;
        });
    }

    private (int Current, int Total) GetNextCarouselPosition()
    {
        var position = GetCarouselPosition();
        var nextCurrent = position.Current == position.Total ? 1 : position.Current + 1;
        return (nextCurrent, position.Total);
    }

    private void ClickWithFallback(IWebElement element)
    {
        Wait.Until(_ => element.Displayed && element.Enabled);

        try
        {
            element.Click();
        }
        catch (ElementClickInterceptedException)
        {
            new Actions(Driver).MoveToElement(element).Click().Perform();
        }
    }

    private (string Url, IReadOnlyCollection<string> Handles) CaptureNavigationState()
    {
        return (Driver.Url, Driver.WindowHandles);
    }

    private void WaitForNavigation((string Url, IReadOnlyCollection<string> Handles) before)
    {
        var wait = CreateWait(20);

        wait.Until(d =>
        {
            if (d.WindowHandles.Count > before.Handles.Count)
            {
                var newHandle = d.WindowHandles.Except(before.Handles).First();
                d.SwitchTo().Window(newHandle);
                return true;
            }

            return !string.Equals(d.Url, before.Url, StringComparison.OrdinalIgnoreCase);
        });
    }

    private void EnsureInsightsLoaded()
    {
        Wait.Until(d => d.Url.Contains("/insights", StringComparison.OrdinalIgnoreCase));
        _ = GetAndValidateCarouselRoot();
    }

    private IWebElement GetAndValidateCarouselRoot()
    {
        if (TryGetValidCachedRoot(out var cached))
        {
            return cached;
        }

        cachedRoot = Wait.Until(driver => FindValidRootFromVisibleArrows(driver));
        return cachedRoot ?? throw new NoSuchElementException("Featured stories carousel root not found.");
    }

    private bool TryGetValidCachedRoot(out IWebElement root)
    {
        root = null!;

        if (cachedRoot == null)
        {
            return false;
        }

        try
        {
            if (!cachedRoot.Displayed)
            {
                return false;
            }

            root = cachedRoot;
            return true;
        }
        catch (StaleElementReferenceException)
        {
            cachedRoot = null;
            return false;
        }
    }

    private static IWebElement? FindValidRootFromVisibleArrows(IWebDriver driver)
    {
        var arrows = driver.FindElements(nextArrowBy).Where(a => a.Displayed);

        foreach (var arrow in arrows)
        {
            var root = arrow.FindElements(ancestorRootBy).FirstOrDefault();

            if (IsValidCarouselRoot(root))
            {
                return root;
            }
        }

        return null;
    }

    private static bool IsValidCarouselRoot(IWebElement? root)
    {
        if (root == null || !root.Displayed)
        {
            return false;
        }

        var hasCounter = root.FindElements(counterElementBy)
            .Select(e => Normalize(e.Text))
            .Any(t => counterRegex.IsMatch(t));
        return hasCounter;
    }

    private IWebElement GetClickableNextArrowButton()
    {
        var root = GetAndValidateCarouselRoot();

        var arrow = Wait.Until(_ => root.FindElements(nextArrowBy).FirstOrDefault(e => e.Displayed && e.Enabled));
        if (arrow == null)
        {
            throw new NoSuchElementException("Carousel next arrow not found.");
        }

        return arrow;
    }

    private (int Current, int Total) GetCarouselPosition()
    {
        var root = GetAndValidateCarouselRoot();

        var counterText = Wait.Until(_ =>
        {
            var candidate = root.FindElements(counterElementBy)
                .Select(e => Normalize(e.Text))
                .FirstOrDefault(t => counterRegex.IsMatch(t));

            return candidate;
        });

        if (string.IsNullOrWhiteSpace(counterText))
        {
            throw new InvalidOperationException("Carousel counter not found.");
        }

        var parts = counterText.Split('/');
        if (parts.Length != 2 ||
            !int.TryParse(parts[0].Trim(), out var current) ||
            !int.TryParse(parts[1].Trim(), out var total))
        {
            throw new InvalidOperationException($"Unexpected carousel counter format: {counterText}");
        }

        return (current, total);
    }

    private IWebElement GetActiveCtaElement()
    {
        var root = GetAndValidateCarouselRoot();

        var cta = root.FindElements(ctaBy).FirstOrDefault(IsUsableCta);
        if (cta == null)
        {
            throw new NoSuchElementException("No usable Read More link found in carousel.");
        }

        return cta;
    }

    private static bool IsUsableCta(IWebElement element)
    {
        if (!element.Displayed || !element.Enabled)
        {
            return false;
        }

        if (string.IsNullOrWhiteSpace(element.GetAttribute("href")))
        {
            return false;
        }

        var insideCloned = element.FindElements(ancestorSlickClonedBy).Count > 0;
        return !insideCloned;
    }

    private string GetAndValidateTitleForCta(IWebElement cta)
    {
        var scope = GetAndValidateCardContainerForCta(cta);

        var title = Wait.Until(_ =>
        {
            var candidate = GetTitleCandidates(scope).FirstOrDefault();
            if (!string.IsNullOrWhiteSpace(candidate.Text))
            {
                return candidate.Text;
            }
            return null;
        });

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new NoSuchElementException("Title element not found for CTA card.");
        }

        return title;
    }

    private static List<(IWebElement Element, string Text)> GetTitleCandidates(IWebElement scope)
    {
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
        var richContainer = cta.FindElements(ancestorRichContainerBy)
            .FirstOrDefault();

        if (richContainer != null)
        {
            return richContainer;
        }

        throw new NoSuchElementException("Rich container not found for CTA.");
    }
}