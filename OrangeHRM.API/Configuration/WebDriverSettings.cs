namespace OrangeHRM.API.Configuration;

/// <summary>
/// Настройки для Selenium WebDriver
/// </summary>
public class WebDriverSettings
{
    public const string SectionName = "WebDriver";

    /// <summary>
    /// Запускать браузер в headless режиме (без GUI)
    /// </summary>
    public bool Headless { get; set; } = false;

    /// <summary>
    /// Неявное ожидание элементов (секунды)
    /// </summary>
    public int ImplicitWaitSeconds { get; set; } = 10;

    /// <summary>
    /// Таймаут загрузки страницы (секунды)
    /// </summary>
    public int PageLoadTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Таймаут выполнения команд (секунды)
    /// </summary>
    public int CommandTimeoutSeconds { get; set; } = 60;
}
