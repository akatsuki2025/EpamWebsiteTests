using OpenQA.Selenium;
using System.Text.RegularExpressions;

namespace EpamWebsiteTests.BusinessLayer.PageObjects;

public class ArticlePage : BasePage
{
    public ArticlePage(IWebDriver driver) : base(driver) { }

    private static string Normalize(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return string.Empty;
        return Regex.Replace(text, "\\s+", " ").Trim();
    }

    public string GetArticleTitle()
    {
        Wait.Until(d =>
            ((IJavaScriptExecutor)d).ExecuteScript("return document.readyState")?.ToString() == "complete");

        var titleSelectors = new[]
        {
            "main h1, article h1, h1",
            ".single-section-full-width__content-container .scaling-of-text-wrapper",
            ".single-section-full-width__content-container .text-ui-23 p",
            ".single-section-full-width__content-container .font-size-80-33"
        };

        var title = Wait.Until(d =>
        {
            var js = (IJavaScriptExecutor)d;

            foreach (var selector in titleSelectors)
            {
                var element = d.FindElements(By.CssSelector(selector))
                    .FirstOrDefault(e => e.Displayed);

                // just use element.Text;
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
                    return normalized;
                }
            }

            return null;
        });

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new NoSuchElementException("Article title not found.");
        }

        return title;
    }
}