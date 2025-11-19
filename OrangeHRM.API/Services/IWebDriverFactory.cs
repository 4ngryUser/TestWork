using OpenQA.Selenium;

namespace OrangeHRM.API.Services;

/// <summary>
/// Фабрика для создания экземпляров WebDriver
/// </summary>
public interface IWebDriverFactory
{
    IWebDriver CreateDriver();
}