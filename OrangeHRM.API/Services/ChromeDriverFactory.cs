using Microsoft.Extensions.Options;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OrangeHRM.API.Configuration;

namespace OrangeHRM.API.Services;

/// <summary>
/// Фабрика для создания Chrome WebDriver с настройками из конфигурации
/// </summary>
public class ChromeDriverFactory : IWebDriverFactory
{
    private readonly WebDriverSettings _settings;
    private readonly ILogger<ChromeDriverFactory> _logger;

    public ChromeDriverFactory(
        IOptions<WebDriverSettings> settings,
        ILogger<ChromeDriverFactory> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    /// <summary>
    /// Создать новый экземпляр ChromeDriver
    /// </summary>
    public IWebDriver CreateDriver()
    {
        _logger.LogInformation("Создание ChromeDriver. Headless: {Headless}", _settings.Headless);

        var options = new ChromeOptions();

        // Headless режим (без GUI)
        if (_settings.Headless)
        {
            options.AddArgument("--headless=new");
        }

        // Дополнительные аргументы для стабильности
        options.AddArgument("--no-sandbox");
        options.AddArgument("--disable-dev-shm-usage");
        options.AddArgument("--disable-gpu");
        options.AddArgument("--window-size=1920,1080");

        // Отключение логов браузера для чистоты вывода
        options.AddArgument("--log-level=3");
        options.AddExcludedArgument("enable-logging");

        var driver = new ChromeDriver(options);

        // Настройка таймаутов
        driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(_settings.ImplicitWaitSeconds);
        driver.Manage().Timeouts().PageLoad = TimeSpan.FromSeconds(_settings.PageLoadTimeoutSeconds);

        _logger.LogInformation("ChromeDriver успешно создан");

        return driver;
    }
}
