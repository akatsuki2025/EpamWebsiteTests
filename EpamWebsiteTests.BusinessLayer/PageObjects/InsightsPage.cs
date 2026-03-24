using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System.Text.RegularExpressions;

namespace EpamWebsiteTests.BusinessLayer.PageObjects;

public class InsightsPage : BasePage
{
    private static readonly Regex counterRegex = new(@"^\d{1,2}\s*/\s*\d{1,2}$",
        RegexOptions.Compiled);
    private static readonly Regex actionTextRegex = new(@"^(read(\s+the)?\s+(more|report)|learn\s+more|discover|explore|watch|view|see\s+more)$",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);
    private static readonly By nextArrowBy = By.CssSelector("button.slider__right-arrow.slider-navigation-arrow");
    private static readonly By ctaBy = By.CssSelector("a.slider-cta-link[href]");
    private static readonly By counterElementXPath = By.XPath(".//*[contains(normalize-space(.),'/')]");
    private static readonly By ancestorRootXPath = By.XPath(
        "./ancestor::*[.//a[contains(@class,'slider-cta-link')] and .//*[contains(@class,'slick-slide') or contains(@class,'single-slide')]][1]");

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
        var title = GetTitleForCta(cta);
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new NoSuchElementException("Could not resolve title from active carousel card.");
        }

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
        wait.Until(_ => condition(GetCarouselPosition()));
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
        _ = GetCarouselRoot();
    }

    private IWebElement GetCarouselRoot()
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
            var root = arrow.FindElements(ancestorRootXPath).FirstOrDefault();

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

        return root.FindElements(counterElementXPath)
            .Select(e => Normalize(e.Text))
            .Any(t => counterRegex.IsMatch(t));
    }

    private IWebElement GetClickableNextArrowButton()
    {
        var root = GetCarouselRoot();

        return Wait.Until(_ =>
            root.FindElements(nextArrowBy).FirstOrDefault(e => e.Displayed && e.Enabled))
            ?? throw new NoSuchElementException("Carousel next arrow not found.");
    }

    private (int Current, int Total) GetCarouselPosition()
    {
        var root = GetCarouselRoot();

        var counterText = Wait.Until(_ =>
        {
            var candidate = root.FindElements(counterElementXPath)
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
        var root = GetCarouselRoot();

        return Wait.Until(_ =>
        {
            var usable = root.FindElements(ctaBy).Where(IsUsableCta).ToList();
            return usable.FirstOrDefault(IsInsideActiveSlide) ?? usable.FirstOrDefault();
        }) ?? throw new NoSuchElementException("No usable Read More link found in carousel.");
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

        var insideCloned = element.FindElements(By.XPath("./ancestor::*[contains(@class,'slick-cloned')]")).Count > 0;
        return !insideCloned;
    }

    private static bool IsInsideActiveSlide(IWebElement cta)
    {
        return cta.FindElements(By.XPath(
            "./ancestor::*[(contains(@class,'slick-active') or @aria-hidden='false') and not(contains(@class,'slick-cloned'))]"))
            .Count > 0;
    }

    private string GetTitleForCta(IWebElement cta)
    {
        var scope = GetCardContainerForCta(cta);
        var wait = CreateWait(6);

        var title = wait.Until(_ =>
        {
            var candidates = GetTitleCandidates(scope);

            return TryGetClassBasedTitle(candidates)
                ?? TryGetLongestTitle(candidates);
        });

        if (!string.IsNullOrWhiteSpace(title))
        {
            return title;
        }

        throw new NoSuchElementException("Title element not found for CTA card.");
    }

    private static List<(IWebElement Element, string Text)> GetTitleCandidates(IWebElement scope)
    {
        return scope
            .FindElements(By.XPath(
                ".//*[self::h1 or self::h2 or self::h3 or self::p or self::div or self::span or contains(@class,'scaling-of-text-wrapper') or contains(@class,'font-size-')]"))
            .Where(e => e.Displayed && e.FindElements(By.XPath("./ancestor::a|./ancestor::button")).Count == 0)
            .Select(e => (Element: e, Text: Normalize(e.Text)))
            .Where(x => IsLikelyTitleText(x.Text))
            .ToList();
    }

    private static string? TryGetClassBasedTitle(IEnumerable<(IWebElement Element, string Text)> candidates)
    {
        return candidates
            .FirstOrDefault(x =>
            {
                var cls = (x.Element.GetAttribute("class") ?? string.Empty).ToLowerInvariant();
                return cls.Contains("scaling-of-text-wrapper") || cls.Contains("font-size-");
            })
            .Text;
    }

    private static string? TryGetLongestTitle(IEnumerable<(IWebElement Element, string Text)> candidates)
    {
        return candidates
            .OrderByDescending(x => x.Text.Length)
            .FirstOrDefault()
            .Text;
    }

    private IWebElement GetCardContainerForCta(IWebElement cta)
    {
        var richContainer = cta.FindElements(By.XPath(
            "./ancestor::*[.//a[contains(@class,'slider-cta-link')] and (.//h1 or .//h2 or .//h3 or .//*[contains(@class,'scaling-of-text-wrapper')] or .//*[contains(@class,'font-size-')])][1]"))
            .FirstOrDefault();

        if (richContainer != null)
        {
            return richContainer;
        }

        var slide = cta.FindElements(By.XPath(
            "./ancestor::*[contains(@class,'slick-slide') or contains(@class,'single-slide')][1]"))
            .FirstOrDefault();

        if (slide != null)
        {
            return slide;
        }

        var root = GetCarouselRoot();
        return root;
    }

    private static bool IsLikelyTitleText(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return false;
        if (text.Length < 12) return false;
        if (counterRegex.IsMatch(text)) return false;
        if (actionTextRegex.IsMatch(text)) return false;
        if (text.StartsWith("Read ", StringComparison.OrdinalIgnoreCase)) return false;
        if (text.StartsWith("Learn ", StringComparison.OrdinalIgnoreCase)) return false;
        if (string.Equals(text, "undefined", StringComparison.OrdinalIgnoreCase)) return false;
        if (Uri.IsWellFormedUriString(text, UriKind.Absolute)) return false;

        return true;
    }

    private static string Normalize(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        text = text.Replace('\u00A0', ' ');
        return Regex.Replace(text, "\\s+", " ").Trim();
    }
}