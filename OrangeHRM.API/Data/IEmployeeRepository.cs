namespace OrangeHRM.API.Data;

/// <summary>
/// Репозиторий для работы с данными сотрудников
/// </summary>
public interface IEmployeeRepository
{
    Task SaveEmployeeAsync(Employee employee);

    Task<Employee?> GetByEmployeeIdAsync(string employeeId);
}
