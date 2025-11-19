using OrangeHRM.API.Models;

namespace OrangeHRM.API.Services;

/// <summary>
/// Сервис для работы с претензиями
/// </summary>
public interface IClaimService
{
    /// <summary>
    /// Создать претензию для сотрудника
    /// </summary>
    /// <param name="request">Данные претензии</param>
    /// <returns>Результат создания с referenceId или сообщением об ошибке</returns>
    Task<ClaimResponse> CreateClaimAsync(ClaimRequest request);
}
