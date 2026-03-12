using EpamWebsiteTests.BusinessLayer.PageObjects;
using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System.Text.RegularExpressions;

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
        var h1 = Wait.Until(d =>
            d.FindElements(By.CssSelector("main h1, article h1, h1"))
             .FirstOrDefault(e => e.Displayed && !string.IsNullOrWhiteSpace(e.Text)));

        if (h1 == null)
            throw new NoSuchElementException("Article title not found.");

        return Normalize(h1.Text);
    }
}