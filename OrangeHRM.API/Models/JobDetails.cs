using System.ComponentModel.DataAnnotations;

namespace OrangeHRM.API.Models;

/// <summary>
/// Детали работы сотрудника
/// </summary>
public class JobDetails
{
    [Required(ErrorMessage = "Job Title обязателен")]
    public string JobTitle { get; set; } = string.Empty;

    [Required(ErrorMessage = "Job Category обязателен")]
    public string JobCategory { get; set; } = string.Empty;

    [Required(ErrorMessage = "Sub Unit обязателен")]
    public string SubUnit { get; set; } = string.Empty;

    [Required(ErrorMessage = "Location обязателен")]
    public string Location { get; set; } = string.Empty;

    [Required(ErrorMessage = "Employment Status обязателен")]
    public string EmploymentStatus { get; set; } = string.Empty;
}
