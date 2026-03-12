using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System.Text.RegularExpressions;

namespace EpamWebsiteTests.BusinessLayer.PageObjects;

public class InsightsPage : BasePage
{
    private readonly By nextArrowBy = By.CssSelector("button.slider__right-arrow.slider-navigation-arrow");
    private readonly Regex counterRegex = new(@"^\d{1,2}\s*/\s*\d{1,2}$", RegexOptions.Compiled);

    public InsightsPage(IWebDriver driver) : base(driver)
    {
    }

    private static string Normalize(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        text = text.Replace('\u00A0', ' ');
        return Regex.Replace(text, "\\s+", " ").Trim();
    }

    private IWebElement GetNextArrow()
    {
        return Wait.Until(driver =>
            driver.FindElements(nextArrowBy).FirstOrDefault(element => element.Displayed && element.Enabled));
    }

    private IWebElement GetCarouselRoot()
    {
        var arrow = GetNextArrow();

        ((IJavaScriptExecutor)Driver).ExecuteScript(
            "arguments[0].scrollIntoView({block:'end', inline:'nearest'});", arrow);

        return Wait.Until(_ =>
            arrow.FindElements(By.XPath(
                    "./ancestor::*[" +
                    ".//button[contains(@class,'slider__right-arrow')] and " +
                    "(" +
                    ".//a[contains(@class,'slider-cta-link')] or " +
                    ".//a[contains(@href,'/insights/')] or " +
                    ".//a[contains(@href,'/ai-report-2025/')]" +
                    ")" +
                    "][1]"))
                .FirstOrDefault());
    }

    private string GetVisibleCounterText(IWebElement root)
    {
        return Wait.Until(_ =>
        {
            var candidates = root.FindElements(By.XPath(".//*[contains(normalize-space(.),'/')]"));

            foreach (var element in candidates)
            {
                if (!element.Displayed)
                    continue;

                var text = Normalize(element.Text);
                if (counterRegex.IsMatch(text))
                    return text;
            }

            return null;
        });
    }

    private (int Current, int Total) GetCarouselPosition(IWebElement root)
    {
        var counterText = GetVisibleCounterText(root);
        var parts = counterText.Split('/');

        return (int.Parse(parts[0].Trim()), int.Parse(parts[1].Trim()));
    }

    private IWebElement GetActiveSlideContainer()
    {
        var root = GetCarouselRoot();

        return Wait.Until(_ =>
        {
            var active = root.FindElements(By.XPath(
                    ".//*[contains(@class,'single-slide') and (" +
                    "@aria-hidden='false' or " +
                    "contains(@class,'slick-active') or " +
                    "contains(@class,'active'))]"))
                .FirstOrDefault(element => element.Displayed);

            if (active != null)
                return active;

            return root.FindElements(By.XPath(".//*[contains(@class,'single-slide')]"))
                .FirstOrDefault(element => element.Displayed);
        });
    }

    public void ClickCarouselNextButton()
    {
        var root = GetCarouselRoot();
        var before = GetCarouselPosition(root);
        var expectedCurrent = before.Current == before.Total ? 1 : before.Current + 1;
        var next = GetNextArrow();

        ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].click();", next);

        var shortWait = new WebDriverWait(Driver, TimeSpan.FromSeconds(5));
        shortWait.Until(_ =>
        {
            var current = GetCarouselPosition(root);
            return current.Current == expectedCurrent;
        });
    }

    private string GetElementTextRobust(IWebElement element)
    {
        var js = (IJavaScriptExecutor)Driver;

        var innerText = js.ExecuteScript("return arguments[0].innerText || '';", element)?.ToString();
        if (!string.IsNullOrWhiteSpace(innerText))
            return Normalize(innerText);

        var textContent = js.ExecuteScript("return arguments[0].textContent || '';", element)?.ToString();
        return Normalize(textContent ?? string.Empty);
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
                return ctaInActiveSlide;

            var root = GetCarouselRoot();

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

    private IWebElement GetVisibleTitleCard()
    {
        var cta = GetActiveCtaElement();

        return Wait.Until(_ =>
            cta.FindElements(By.XPath(
                    "./ancestor::*[" +
                    ".//span[contains(@class,'font-size-60')] or " +
                    ".//h1 or .//h2 or .//h3" +
                    "][1]"))
                .FirstOrDefault());
    }

    private IWebElement GetVisibleTitleElement()
    {
        var card = GetVisibleTitleCard();

        return Wait.Until(_ =>
        {
            var exact = card.FindElements(By.CssSelector("div.text span.font-size-60"))
                .FirstOrDefault(element =>
                    element.Displayed &&
                    !string.IsNullOrWhiteSpace(GetElementTextRobust(element)));

            if (exact != null)
                return exact;

            return card.FindElements(By.XPath(
                    ".//*[contains(@class,'font-size-60') or contains(@class,'single-slide__title') or self::h1 or self::h2 or self::h3]"))
                .FirstOrDefault(element =>
                    element.Displayed &&
                    !string.IsNullOrWhiteSpace(GetElementTextRobust(element)) &&
                    !counterRegex.IsMatch(Normalize(GetElementTextRobust(element))));
        });
    }

    public string GetCurrentCarouselArticleTitle()
    {
        var titleElement = GetVisibleTitleElement();
        var title = GetElementTextRobust(titleElement);

        if (string.IsNullOrWhiteSpace(title))
            throw new NoSuchElementException("Visible carousel title text is empty.");

        return title;
    }

    public void ClickReadMoreButton()
    {
        var cta = GetActiveCtaElement();

        ((IJavaScriptExecutor)Driver).ExecuteScript(
            "arguments[0].scrollIntoView({block:'end', inline:'nearest'});", cta);
        ((IJavaScriptExecutor)Driver).ExecuteScript("arguments[0].click();", cta);
    }
}