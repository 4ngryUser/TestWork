using Microsoft.EntityFrameworkCore;

namespace OrangeHRM.API.Data;

/// <summary>
/// Репозиторий для работы с данными сотрудников в БД
/// </summary>
public class EmployeeRepository : IEmployeeRepository
{
    private readonly AppDbContext _context;
    private readonly ILogger<EmployeeRepository> _logger;

    public EmployeeRepository(AppDbContext context, ILogger<EmployeeRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Сохранить сотрудника в БД
    /// </summary>
    public async Task SaveEmployeeAsync(Employee employee)
    {
        try
        {
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
            _logger.LogInformation("Сотрудник сохранен в БД: {EmployeeId}", employee.EmployeeId);
        }
        catch (DbUpdateException ex)
        {
            _logger.LogError(ex, "Ошибка при сохранении сотрудника в БД: {EmployeeId}", employee.EmployeeId);
            throw;
        }
    }

    /// <summary>
    /// Найти сотрудника по EmployeeId
    /// </summary>
    public async Task<Employee?> GetByEmployeeIdAsync(string employeeId)
    {
        try
        {
            var employee = await _context.Employees
                .FirstOrDefaultAsync(e => e.EmployeeId == employeeId);

            if (employee != null)
            {
                _logger.LogDebug("Сотрудник найден в БД: {EmployeeId}", employeeId);
            }
            else
            {
                _logger.LogWarning("Сотрудник не найден в БД: {EmployeeId}", employeeId);
            }

            return employee;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при поиске сотрудника в БД: {EmployeeId}", employeeId);
            throw;
        }
    }
}
