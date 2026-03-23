using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System.Text.RegularExpressions;

namespace EpamWebsiteTests.BusinessLayer.PageObjects;

public class InsightsPage : BasePage
{
    private static readonly Regex CounterRegex = new(@"^\d{1,2}\s*/\s*\d{1,2}$", RegexOptions.Compiled);

    private readonly By nextArrowBy = By.CssSelector("button.slider__right-arrow.slider-navigation-arrow");
    private readonly By featuredStoriesCarouselContainerBy = By.XPath(@"./ancestor::*[
        .//button[contains(@class,'slider__right-arrow')] and
        .//a[contains(@class,'slider-cta-link')]
    ][1]");
    private readonly By activeTitleBy = By.CssSelector("div.text span.font-size-60");

    public InsightsPage(IWebDriver driver) : base(driver)
    {
    }

    public void SwipeCarouselNext(int swipeCount)
    {
        if (swipeCount < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(swipeCount));
        }

        for (int i = 0; i < swipeCount; i++)
        {
            ClickCarouselNextButton();
        }
    }

    public string GetActiveCarouselArticleTitle()
    {
        var titleElement = GetActiveTitleElement();
        var title = GetNormalizedTextFromElement(titleElement);

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new NoSuchElementException("Visible carousel title text is empty.");
        }

        return title;
    }

    public void ClickReadMoreButton()
    {
        var cta = GetActiveCtaElement();
        var oldUrl = Driver.Url;
        var oldHandles = Driver.WindowHandles;

        ((IJavaScriptExecutor)Driver).ExecuteScript(
            "arguments[0].scrollIntoView({block:'center', inline:'nearest'});", cta);

        Wait.Until(_ => cta.Displayed && cta.Enabled);
        cta.Click();

        var navWait = new WebDriverWait(Driver, TimeSpan.FromSeconds(20));
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

        navWait.Until(d =>
            ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState")?.ToString() == "complete");
    }

    private void ClickCarouselNextButton()
    {
        var before = GetCarouselPosition();
        var expectedCurrent = before.Current == before.Total ? 1 : before.Current + 1;
        var next = GetClickableNextArrowButton();

        Wait.Until(_ => next.Displayed && next.Enabled);
        next.Click();

        var shortWait = new WebDriverWait(Driver, TimeSpan.FromSeconds(3));
        shortWait.Until(_ =>
        {
            var current = GetCarouselPosition();
            return current.Current == expectedCurrent;
        });
    }

    private (int Current, int Total) GetCarouselPosition()
    {
        var counterText = GetVisibleCounterText();

        if (string.IsNullOrWhiteSpace(counterText))
        {
            throw new InvalidOperationException("Insights carousel counter was not found.");
        }

        var parts = counterText.Split('/');
        if (parts.Length != 2 ||
            !int.TryParse(parts[0].Trim(), out var current) ||
            !int.TryParse(parts[1].Trim(), out var total))
        {
            throw new InvalidOperationException($"Unexpected insights carousel counter format: '{counterText}'.");
        }

        return (current, total);
    }

    private string GetVisibleCounterText()
    {
        var carouselContainer = GetFeaturedStoriesCarouselContainer();

        return Wait.Until(_ =>
        {
            var candidates = carouselContainer.FindElements(By.XPath(".//*[contains(normalize-space(.),'/')]"));

            foreach (var element in candidates)
            {
                if (!element.Displayed)
                {
                    continue;
                }

                var text = Normalize(element.Text);
                if (CounterRegex.IsMatch(text))
                {
                    return text;
                }
            }

            return null;
        });
    }

    private IWebElement GetFeaturedStoriesCarouselContainer()
    {
        var arrow = GetClickableNextArrowButton();

        ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].scrollIntoView({block:'end', inline:'nearest'});", arrow);

        return Wait.Until(_ => arrow.FindElements(featuredStoriesCarouselContainerBy).FirstOrDefault());
    }

    private IWebElement GetClickableNextArrowButton()
    {
        return Wait.Until(driver =>
            driver.FindElements(nextArrowBy)
                .FirstOrDefault(element => element.Displayed && element.Enabled));
    }

    private IWebElement GetActiveTitleElement()
    {
        var titleCard = GetActiveTitleCard();

        return Wait.Until(_ =>
            titleCard.FindElements(activeTitleBy)
                .FirstOrDefault(IsVisibleTitleCandidate));
    }

    private IWebElement GetActiveTitleCard()
    {
        var cta = GetActiveCtaElement();

        return Wait.Until(_ =>
            cta.FindElements(By.XPath("./ancestor::*[.//span[contains(@class,'font-size-60')]][1]"))
                .FirstOrDefault());
    }

    private IWebElement GetActiveCtaElement()
    {
        var activeSlide = GetActiveSlideContainer();

        return Wait.Until(_ =>
        {
            var ctaInActiveSlide = activeSlide.FindElements(By.XPath(
                    ".//a[" +
                    "@href and not(starts-with(@href,'#')) and (" +
                    "contains(@class,'slider-cta-link') or " +
                    "contains(@href,'/insights/') or " +
                    "contains(@href,'/ai-report-2025/')" +
                    ")]"))
                .FirstOrDefault(element => element.Displayed && element.Enabled);

            if (ctaInActiveSlide != null)
            {
                return ctaInActiveSlide;
            }

            var root = GetFeaturedStoriesCarouselContainer();

            return root.FindElements(By.XPath(
                    ".//a[" +
                    "@href and not(starts-with(@href,'#')) and (" +
                    "contains(@class,'slider-cta-link') or " +
                    "contains(@href,'/insights/') or " +
                    "contains(@href,'/ai-report-2025/')" +
                    ")]"))
                .FirstOrDefault(element => element.Displayed && element.Enabled);
        });
    }

    private IWebElement GetActiveSlideContainer()
    {
        var root = GetFeaturedStoriesCarouselContainer();

        return Wait.Until(_ =>
        {
            var active = root.FindElements(By.XPath(
                    ".//*[contains(@class,'single-slide') and (" +
                    "@aria-hidden='false' or " +
                    "contains(@class,'slick-active') or " +
                    "contains(@class,'active'))]"))
                .FirstOrDefault(element => element.Displayed);

            if (active != null)
            {
                return active;
            }

            return root.FindElements(By.XPath(".//*[contains(@class,'single-slide')]"))
                .FirstOrDefault(element => element.Displayed);
        });
    }

    private bool IsVisibleTitleCandidate(IWebElement element)
    {
        if (!element.Displayed)
        {
            return false;
        }

        var text = GetNormalizedTextFromElement(element);
        return !string.IsNullOrWhiteSpace(text) && !CounterRegex.IsMatch(text);
    }

    private string GetNormalizedTextFromElement(IWebElement element)
    {
        var js = (IJavaScriptExecutor)Driver;

        var innerText = js.ExecuteScript("return arguments[0].innerText || '';", element)?.ToString();
        if (!string.IsNullOrWhiteSpace(innerText))
        {
            return Normalize(innerText);
        }

        var textContent = js.ExecuteScript("return arguments[0].textContent || '';", element)?.ToString();
        return Normalize(textContent ?? string.Empty);
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