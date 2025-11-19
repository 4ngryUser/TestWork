using System.ComponentModel.DataAnnotations;

namespace OrangeHRM.API.Models;

/// <summary>
/// Запрос на добавление нового сотрудника
/// </summary>
public class EmployeeRequest
{
    [Required(ErrorMessage = "First Name обязателен")]
    [StringLength(100, ErrorMessage = "First Name не может быть длиннее 100 символов")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Middle Name обязателен")]
    [StringLength(100, ErrorMessage = "Middle Name не может быть длиннее 100 символов")]
    public string MiddleName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last Name обязателен")]
    [StringLength(100, ErrorMessage = "Last Name не может быть длиннее 100 символов")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Job детали обязательны")]
    public JobDetails Job { get; set; } = new();
}
