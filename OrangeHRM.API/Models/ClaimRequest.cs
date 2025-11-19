using System.ComponentModel.DataAnnotations;

namespace OrangeHRM.API.Models;

/// <summary>
/// Запрос на создание претензии
/// </summary>
public class ClaimRequest
{

    public string? EmployeeName { get; set; }
    public string? EmployeeId { get; set; }

    [Required(ErrorMessage = "Event обязателен")]
    public string Event { get; set; } = string.Empty;

    [Required(ErrorMessage = "Currency обязателен")]
    public string Currency { get; set; } = string.Empty;

    public string? Remarks { get; set; }
}
