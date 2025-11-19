using Microsoft.Extensions.Options;
using OpenQA.Selenium;
using OrangeHRM.API.Configuration;
using OrangeHRM.API.Helpers;

namespace OrangeHRM.API.Services;

/// <summary>
/// Сервис авторизации на OrangeHRM Demo
/// </summary>
public class AuthService : IAuthService
{
    private readonly OrangeHRMSettings _settings;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IOptions<OrangeHRMSettings> settings,
        ILogger<AuthService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    /// <summary>
    /// Выполнить авторизацию на сайте OrangeHRM
    /// </summary>
    public async Task<bool> LoginAsync(IWebDriver driver)
    {
        try
        {
            _logger.LogInformation("Начало авторизации на {BaseUrl}", _settings.BaseUrl);

            // Переход на страницу авторизации
            driver.Navigate().GoToUrl(_settings.BaseUrl);
            _logger.LogDebug("Переход на страницу авторизации выполнен");

            // Ожидание и заполнение поля Username
            var usernameLocator = By.Name("username");
            WebAutomationHelper.WaitAndSendKeys(driver, usernameLocator, _settings.Username);
            _logger.LogDebug("Поле Username заполнено");

            // Заполнение поля Password
            var passwordLocator = By.Name("password");
            WebAutomationHelper.WaitAndSendKeys(driver, passwordLocator, _settings.Password);
            _logger.LogDebug("Поле Password заполнено");

            // Нажатие кнопки Login
            var loginButtonLocator = By.CssSelector("button[type='submit']");
            WebAutomationHelper.WaitAndClick(driver, loginButtonLocator);
            _logger.LogDebug("Кнопка Login нажата");

            // Ожидание успешной авторизации (появление dashboard)
            // После логина должен появиться элемент Dashboard или главное меню
            var dashboardLocator = By.CssSelector("h6.oxd-topbar-header-breadcrumb-module, span.oxd-topbar-header-breadcrumb");

            // Даем время на загрузку
            await Task.Delay(2000);

            var dashboardElement = WebAutomationHelper.WaitForElement(driver, dashboardLocator, timeoutSeconds: 10);

            if (dashboardElement != null)
            {
                _logger.LogInformation("Авторизация успешно выполнена");
                return true;
            }

            _logger.LogWarning("Элемент dashboard не найден после авторизации");
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при авторизации на OrangeHRM");
            return false;
        }
    }
}