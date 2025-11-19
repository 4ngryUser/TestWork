namespace OrangeHRM.API.Helpers;

/// <summary>
/// Генератор уникальных Employee ID
/// </summary>
public static class EmployeeIdGenerator
{
    private const string AllowedCharacters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private const int IdLength = 7;

    /// <summary>
    /// Сгенерировать случайный Employee ID из 7 символов (буквы и цифры)
    /// </summary>
    /// <returns>Случайная строка из 7 символов</returns>
    public static string Generate()
    {
        var random = new Random();
        var chars = new char[IdLength];

        for (int i = 0; i < IdLength; i++)
        {
            chars[i] = AllowedCharacters[random.Next(AllowedCharacters.Length)];
        }

        return new string(chars);
    }

    /// <summary>
    /// Сгенерировать уникальный Employee ID с проверкой на уникальность
    /// </summary>
    /// <param name="isUnique">Функция проверки уникальности ID</param>
    /// <param name="maxAttempts">Максимальное количество попыток генерации</param>
    /// <returns>Уникальный Employee ID или null если не удалось сгенерировать за maxAttempts попыток</returns>
    public static string? GenerateUnique(Func<string, bool> isUnique, int maxAttempts = 10)
    {
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            var employeeId = Generate();

            if (isUnique(employeeId))
            {
                return employeeId;
            }
        }

        // Не удалось сгенерировать уникальный ID за maxAttempts попыток
        return null;
    }
}
