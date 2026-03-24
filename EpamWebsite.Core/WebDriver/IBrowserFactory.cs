using OpenQA.Selenium;

namespace EpamWebsite.Core.WebDriver;

public interface IBrowserFactory
{
    WebDriverSession Create(string downloadDirectory);
}