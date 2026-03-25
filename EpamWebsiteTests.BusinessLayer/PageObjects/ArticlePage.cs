using OpenQA.Selenium;
using System.Text.RegularExpressions;
using Serilog;

namespace EpamWebsiteTests.BusinessLayer.PageObjects;

public class ArticlePage : BasePage
{
    public ArticlePage(IWebDriver driver) : base(driver) { }

    public string GetArticleTitle()
    {
        Log.Information("Attempting to get article title from the page.");

        Wait.Until(d =>
            ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState")?.ToString() == "complete");

        var titleSelectors = new[]
        {
            "main h1, article h1, h1",
            ".single-section-full-width__content-container .scaling-of-text-wrapper"
        };

        var title = Wait.Until(d =>
        {
            var js = (IJavaScriptExecutor)d;

            foreach (var selector in titleSelectors)
            {
                var element = d.FindElements(By.CssSelector(selector))
                    .FirstOrDefault(e => e.Displayed);

                if (element == null)
                {
                    continue;
                }

                var raw = js.ExecuteScript(
                    "return arguments[0].innerText || arguments[0].textContent || '';",
                    element)?.ToString();

                var normalized = Normalize(raw ?? string.Empty);
                if (!string.IsNullOrWhiteSpace(normalized))
                {
                    Log.Information("Found article title using selector '{Selector}': {Title}", selector, normalized);
                    return normalized;
                }
            }

            return null;
        });

        if (string.IsNullOrWhiteSpace(title))
        {
            Log.Error("Article title not found using any of the selectors.");
        }

        return title;
    }
}