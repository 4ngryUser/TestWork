using OrangeHRM.API.Models;

namespace OrangeHRM.API.Services;

/// <summary>
/// Сервис для работы с сотрудниками
/// </summary>
public interface IEmployeeService
{
    /// <summary>
    /// Добавить нового сотрудника в систему OrangeHRM
    /// </summary>
    /// <param name="request">Данные нового сотрудника</param>
    /// <returns>Результат добавления с employeeId или сообщением об ошибке</returns>
    Task<EmployeeResponse> AddEmployeeAsync(EmployeeRequest request);
}
