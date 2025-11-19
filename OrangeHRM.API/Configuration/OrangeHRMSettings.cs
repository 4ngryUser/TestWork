namespace OrangeHRM.API.Configuration;

/// <summary>
/// Настройки для подключения к OrangeHRM Demo
/// </summary>
public class OrangeHRMSettings
{
    public const string SectionName = "OrangeHRM";

    /// <summary>
    /// Базовый URL сайта OrangeHRM
    /// </summary>
    public string BaseUrl { get; set; } = string.Empty;

    /// <summary>
    /// Имя пользователя для авторизации
    /// </summary>
    public string Username { get; set; } = string.Empty;

    /// <summary>
    /// Пароль для авторизации
    /// </summary>
    public string Password { get; set; } = string.Empty;
}
