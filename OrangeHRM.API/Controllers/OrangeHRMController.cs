using Microsoft.AspNetCore.Mvc;
using OrangeHRM.API.Models;
using OrangeHRM.API.Services;

namespace OrangeHRM.API.Controllers;

/// <summary>
/// Контроллер для работы с OrangeHRM Demo
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class OrangeHRMController : ControllerBase
{
    private readonly IEmployeeService _employeeService;
    private readonly IClaimService _claimService;
    private readonly ILogger<OrangeHRMController> _logger;

    public OrangeHRMController(
        IEmployeeService employeeService,
        IClaimService claimService,
        ILogger<OrangeHRMController> logger)
    {
        _employeeService = employeeService;
        _claimService = claimService;
        _logger = logger;
    }

    /// <summary>
    /// Добавить нового сотрудника в систему OrangeHRM
    /// </summary>
    /// <param name="request">Данные нового сотрудника</param>
    /// <returns>Результат добавления с employeeId или сообщением об ошибке</returns>
    /// <response code="200">Сотрудник успешно добавлен или произошла ошибка (в теле ответа success: true/false)</response>
    /// <response code="400">Некорректные входные данные</response>
    [HttpPost("employee")]
    [ProducesResponseType(typeof(EmployeeResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<EmployeeResponse>> AddEmployee([FromBody] EmployeeRequest request)
    {
        _logger.LogInformation("Получен запрос на добавление сотрудника: {FirstName} {LastName}",
            request.FirstName, request.LastName);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Некорректные данные в запросе на добавление сотрудника");
            return BadRequest(ModelState);
        }

        var response = await _employeeService.AddEmployeeAsync(request);

        if (response.Success)
        {
            _logger.LogInformation("Сотрудник успешно добавлен. Employee ID: {EmployeeId}", response.EmployeeId);
        }
        else
        {
            _logger.LogWarning("Не удалось добавить сотрудника. Ошибка: {ErrorMessage}", response.ErrorMessage);
        }

        return Ok(response);
    }

    /// <summary>
    /// Создать претензию для сотрудника
    /// </summary>
    /// <param name="request">Данные претензии</param>
    /// <returns>Результат создания с referenceId или сообщением об ошибке</returns>
    /// <response code="200">Претензия успешно создана или произошла ошибка (в теле ответа success: true/false)</response>
    /// <response code="400">Некорректные входные данные</response>
    [HttpPost("claim")]
    [ProducesResponseType(typeof(ClaimResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ClaimResponse>> CreateClaim([FromBody] ClaimRequest request)
    {
        _logger.LogInformation("Получен запрос на создание претензии для сотрудника: {EmployeeName}",
            request.EmployeeName ?? request.EmployeeId);

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("Некорректные данные в запросе на создание претензии");
            return BadRequest(ModelState);
        }

        var response = await _claimService.CreateClaimAsync(request);

        if (response.Success)
        {
            _logger.LogInformation("Претензия успешно создана. Reference ID: {ReferenceId}", response.ReferenceId);
        }
        else
        {
            _logger.LogWarning("Не удалось создать претензию. Ошибка: {ErrorMessage}", response.ErrorMessage);
        }

        return Ok(response);
    }
}
