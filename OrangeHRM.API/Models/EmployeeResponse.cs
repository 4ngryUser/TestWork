namespace OrangeHRM.API.Models;

/// <summary>
/// Ответ после добавления сотрудника
/// </summary>
public class EmployeeResponse
{
    public bool Success { get; set; }

    public string? EmployeeId { get; set; }

    public string? ErrorMessage { get; set; }

    public static EmployeeResponse CreateSuccess(string employeeId)
    {
        return new EmployeeResponse
        {
            Success = true,
            EmployeeId = employeeId
        };
    }

    public static EmployeeResponse CreateError(string errorMessage)
    {
        return new EmployeeResponse
        {
            Success = false,
            ErrorMessage = errorMessage
        };
    }
}