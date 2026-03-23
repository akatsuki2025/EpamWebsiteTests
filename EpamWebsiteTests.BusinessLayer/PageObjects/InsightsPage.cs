using OpenQA.Selenium;
using OpenQA.Selenium.Interactions;
using OpenQA.Selenium.Support.UI;
using System.Text.RegularExpressions;

namespace EpamWebsiteTests.BusinessLayer.PageObjects;

public class InsightsPage : BasePage
{
    private static readonly Regex CounterRegex = new(@"^\d{1,2}\s*/\s*\d{1,2}$", 
        RegexOptions.Compiled);
    private static readonly Regex ActionTextRegex = new(@"^(read(\s+the)?\s+(more|report)|learn\s+more|discover|explore|watch|view|see\s+more)$", 
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private readonly By nextArrowBy = By.CssSelector("button.slider__right-arrow.slider-navigation-arrow");
    private readonly By ctaBy = By.CssSelector("a.slider-cta-link[href]");

    private IWebElement? cachedRoot;
    private string? lastCapturedArticleHref;

    public InsightsPage(IWebDriver driver) : base(driver)
    {
    }

    public void SwipeCarouselNext(int swipeCount)
    {
        if (swipeCount < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(swipeCount));
        }

        EnsureInsightsLoaded();

        for (int i = 0; i < swipeCount; i++)
        {
            var before = GetCarouselPosition();
            var expectedCurrent = before.Current == before.Total ? 1 : before.Current + 1;

            var next = GetClickableNextArrowButton();
            Wait.Until(_ => next.Displayed && next.Enabled);

            try
            {
                next.Click();
            }
            catch (ElementClickInterceptedException)
            {
                new Actions(Driver).MoveToElement(next).Click().Perform();
            }

            WaitForCounterValue(expectedCurrent, before.Total);
        }
    }

    private void WaitForCounterValue(int expectedCurrent, int expectedTotal)
    {
        var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10))
        {
            PollingInterval = TimeSpan.FromMilliseconds(120)
        };
        wait.IgnoreExceptionTypes(typeof(StaleElementReferenceException));

        wait.Until(_ =>
        {
            var pos = GetCarouselPosition();
            return pos.Total == expectedTotal && pos.Current == expectedCurrent;
        });
    }

    public string GetActiveCarouselArticleTitle()
    {
        EnsureInsightsLoaded();
        WaitForCounterStable();

        var cta = GetActiveCtaElement();
        lastCapturedArticleHref = cta.GetAttribute("href");

        var title = GetTitleForCta(cta);
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new NoSuchElementException("Could not resolve title from active carousel card.");
        }

        return title;
    }

    public void ClickReadMoreButton()
    {
        EnsureInsightsLoaded();
        WaitForCounterStable();

        var oldUrl = Driver.Url;
        var oldHandles = Driver.WindowHandles;

        IWebElement cta;
        if (!string.IsNullOrWhiteSpace(lastCapturedArticleHref))
        {
            cta = FindCtaByHref(lastCapturedArticleHref!) ?? GetActiveCtaElement();
        }
        else
        {
            cta = GetActiveCtaElement();
        }

        Wait.Until(_ => cta.Displayed && cta.Enabled);

        try
        {
            cta.Click();
        }
        catch (ElementClickInterceptedException)
        {
            new Actions(Driver).MoveToElement(cta).Pause(TimeSpan.FromMilliseconds(100)).Click().Perform();
        }

        var navWait = new WebDriverWait(Driver, TimeSpan.FromSeconds(20))
        {
            PollingInterval = TimeSpan.FromMilliseconds(150)
        };
        navWait.IgnoreExceptionTypes(typeof(StaleElementReferenceException));

        navWait.Until(d =>
        {
            if (d.WindowHandles.Count > oldHandles.Count)
            {
                var newHandle = d.WindowHandles.Except(oldHandles).First();
                d.SwitchTo().Window(newHandle);
                return true;
            }

            return !string.Equals(d.Url, oldUrl, StringComparison.OrdinalIgnoreCase);
        });

        lastCapturedArticleHref = null;
    }

    private void EnsureInsightsLoaded()
    {
        Wait.Until(d => d.Url.Contains("/insights", StringComparison.OrdinalIgnoreCase));
        _ = GetCarouselRoot();
    }

    private IWebElement GetCarouselRoot()
    {
        if (TryGetCachedRoot(out var cached))
        {
            return cached;
        }

        cachedRoot = Wait.Until(driver =>
        {
            var arrow = FindVisibleArrowWithValidRoot(driver);
            return arrow == null ? null : FindValidRootFromArrow(arrow);
        });

        return cachedRoot ?? throw new NoSuchElementException("Featured stories carousel root not found.");
    }

    private bool TryGetCachedRoot(out IWebElement root)
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

    private IWebElement? FindVisibleArrowWithValidRoot(IWebDriver driver)
    {
        foreach (var arrow in driver.FindElements(nextArrowBy))
        {
            if (!arrow.Displayed)
            {
                continue;
            }

            var root = FindValidRootFromArrow(arrow);
            if (root != null)
            {
                return arrow;
            }
        }

        return null;
    }

    private IWebElement? FindValidRootFromArrow(IWebElement arrow)
    {
        var root = arrow.FindElements(By.XPath(
                "./ancestor::*[.//a[contains(@class,'slider-cta-link')] and .//*[contains(@class,'slick-slide') or contains(@class,'single-slide')]][1]"))
            .FirstOrDefault();

        if (root == null || !root.Displayed)
        {
            return null;
        }

        return HasCounter(root) ? root : null;
    }

    private bool HasCounter(IWebElement root)
    {
        return root.FindElements(By.XPath(".//*[contains(normalize-space(.),'/')]"))
            .Select(e => Normalize(e.Text))
            .Any(t => CounterRegex.IsMatch(t));
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
            var candidate = root.FindElements(By.XPath(".//*[contains(normalize-space(.),'/')]"))
                .Select(e => Normalize(e.Text))
                .FirstOrDefault(t => CounterRegex.IsMatch(t));

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

    private void WaitForCounterStable()
    {
        var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(10))
        {
            PollingInterval = TimeSpan.FromMilliseconds(150)
        };
        wait.IgnoreExceptionTypes(typeof(StaleElementReferenceException));

        string? first = null;
        wait.Until(_ =>
        {
            var pos = GetCarouselPosition();
            var now = $"{pos.Current}/{pos.Total}";

            if (first == null)
            {
                first = now;
                return false;
            }

            return string.Equals(first, now, StringComparison.Ordinal);
        });
    }

    private IWebElement GetActiveCtaElement()
    {
        var root = GetCarouselRoot();

        return Wait.Until(_ =>
        {
            var links = root.FindElements(ctaBy);

            IWebElement? preferred = null;
            IWebElement? fallback = null;

            foreach (var link in links)
            {
                if (!IsUsableCta(link))
                {
                    continue;
                }

                fallback ??= link;

                if (IsInsideActiveSlide(link))
                {
                    preferred = link;
                    break;
                }
            }

            return preferred ?? fallback;
        }) ?? throw new NoSuchElementException("No usable Read More link found in carousel.");
    }

    private IWebElement? FindCtaByHref(string href)
    {
        var root = GetCarouselRoot();

        return Wait.Until(_ =>
        {
            var links = root.FindElements(ctaBy);

            foreach (var link in links)
            {
                if (!IsUsableCta(link))
                {
                    continue;
                }

                var currentHref = link.GetAttribute("href");
                if (string.Equals(currentHref, href, StringComparison.OrdinalIgnoreCase))
                {
                    return link;
                }
            }

            return null;
        });
    }

    private bool IsUsableCta(IWebElement element)
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

    private bool IsInsideActiveSlide(IWebElement cta)
    {
        return cta.FindElements(By.XPath(
            "./ancestor::*[(contains(@class,'slick-active') or @aria-hidden='false') and not(contains(@class,'slick-cloned'))]"))
            .Count > 0;
    }

    private string GetTitleForCta(IWebElement cta)
    {
        var scope = GetCardContainerForCta(cta);

        var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(6))
        {
            PollingInterval = TimeSpan.FromMilliseconds(150)
        };
        wait.IgnoreExceptionTypes(typeof(StaleElementReferenceException));

        var title = wait.Until(_ =>
        {
            var candidates = scope.FindElements(By.XPath(
                ".//*[self::h1 or self::h2 or self::h3 or self::p or self::div or self::span or contains(@class,'scaling-of-text-wrapper') or contains(@class,'font-size-')]"));

            var best = candidates
                .Where(e => e.Displayed)
                .Select(e => new
                {
                    Element = e,
                    Text = Normalize(e.Text)
                })
                .Where(x =>
                    IsLikelyTitleText(x.Text) &&
                    x.Element.FindElements(By.XPath("./ancestor::a|./ancestor::button")).Count == 0)
                .OrderByDescending(x => ScoreTitleCandidate(x.Element, x.Text))
                .FirstOrDefault();

            return best?.Text;
        });

        if (!string.IsNullOrWhiteSpace(title))
        {
            return title;
        }

        throw new NoSuchElementException("Title element not found for CTA card.");
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

    private int ScoreTitleCandidate(IWebElement element, string text)
    {
        if (!IsLikelyTitleText(text))
        {
            return 0;
        }

        var score = 0;

        var tag = (element.TagName ?? string.Empty).ToLowerInvariant();
        if (tag == "h1") score += 300;
        else if (tag == "h2") score += 260;
        else if (tag == "h3") score += 220;
        else if (tag == "p") score += 120;

        var cls = (element.GetAttribute("class") ?? string.Empty).ToLowerInvariant();
        if (cls.Contains("scaling-of-text-wrapper")) score += 280;
        if (cls.Contains("font-size-")) score += 220;

        score += Math.Min(text.Length, 120);

        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;
        if (words < 3) score -= 120;

        return score;
    }

    private bool IsLikelyTitleText(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return false;
        if (text.Length < 12) return false;
        if (CounterRegex.IsMatch(text)) return false;
        if (ActionTextRegex.IsMatch(text)) return false;
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