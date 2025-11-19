using OpenQA.Selenium;

namespace OrangeHRM.API.Services;

/// <summary>
/// Сервис авторизации на OrangeHRM
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Выполнить авторизацию на сайте OrangeHRM
    /// </summary>
    /// <param name="driver">WebDriver для выполнения операций</param>
    /// <returns>True если авторизация успешна, иначе False</returns>
    Task<bool> LoginAsync(IWebDriver driver);
}
