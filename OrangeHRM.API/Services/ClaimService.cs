using OpenQA.Selenium;
using OrangeHRM.API.Data;
using OrangeHRM.API.Helpers;
using OrangeHRM.API.Models;

namespace OrangeHRM.API.Services;

/// <summary>
/// Сервис для работы с претензиями в OrangeHRM
/// </summary>
public class ClaimService : IClaimService
{
    private readonly IWebDriverFactory _driverFactory;
    private readonly IAuthService _authService;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ILogger<ClaimService> _logger;

    public ClaimService(
        IWebDriverFactory driverFactory,
        IAuthService authService,
        IEmployeeRepository employeeRepository,
        ILogger<ClaimService> logger)
    {
        _driverFactory = driverFactory;
        _authService = authService;
        _employeeRepository = employeeRepository;
        _logger = logger;
    }

    /// <summary>
    /// Создать претензию для сотрудника
    /// </summary>
    public async Task<ClaimResponse> CreateClaimAsync(ClaimRequest request)
    {
        IWebDriver? driver = null;

        try
        {
            _logger.LogInformation("Начало создания претензии для сотрудника: {EmployeeName}",
                request.EmployeeName ?? request.EmployeeId);

            // Создание WebDriver
            driver = _driverFactory.CreateDriver();

            // Шаг 1: Авторизация
            var loginSuccess = await _authService.LoginAsync(driver);
            if (!loginSuccess)
            {
                return ClaimResponse.CreateError("Не удалось авторизоваться в системе");
            }

            // Шаг 2: Навигация Claim -> Assign Claim
            await NavigateToAssignClaimPage(driver);

            // Шаг 3: Получение имени сотрудника
            string employeeName;

            if (!string.IsNullOrEmpty(request.EmployeeId))
            {
                // Дополнительное задание: получить ФИО из БД по EmployeeId
                var employee = await _employeeRepository.GetByEmployeeIdAsync(request.EmployeeId);

                if (employee == null)
                {
                    return ClaimResponse.CreateError($"Сотрудник с ID '{request.EmployeeId}' не найден в базе данных");
                }

                employeeName = employee.GetFullName();
                _logger.LogInformation("Получено ФИО из БД для employeeId {EmployeeId}: {EmployeeName}",
                    request.EmployeeId, employeeName);
            }
            else if (!string.IsNullOrEmpty(request.EmployeeName))
            {
                // Базовое задание: использовать переданное имя
                employeeName = request.EmployeeName;
            }
            else
            {
                return ClaimResponse.CreateError("Необходимо указать EmployeeName или EmployeeId");
            }

            // Шаг 4: Выбор сотрудника из автокомплита
            bool employeeSelected = SelectEmployeeFromAutocomplete(driver, employeeName);
            if (!employeeSelected)
            {
                return ClaimResponse.CreateError($"Сотрудник '{employeeName}' не найден в системе OrangeHRM");
            }

            // Шаг 5: Заполнение полей претензии (Event, Currency, Remarks)
            FillClaimDetails(driver, request);

            // Шаг 7: Нажатие кнопки Create
            ClickCreateButton(driver);

            // Ожидание создания претензии
            await Task.Delay(2000);

            // Шаг 8: Парсинг Reference Id со страницы
            var referenceId = ParseReferenceId(driver);
            if (string.IsNullOrEmpty(referenceId))
            {
                return ClaimResponse.CreateError("Не удалось получить Reference ID после создания претензии");
            }

            _logger.LogInformation("Претензия успешно создана с Reference ID: {ReferenceId}", referenceId);

            return ClaimResponse.CreateSuccess(referenceId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при создании претензии");
            return ClaimResponse.CreateError($"Ошибка при создании претензии: {ex.Message}");
        }
        finally
        {
            driver?.Quit();
            driver?.Dispose();
        }
    }

    /// <summary>
    /// Навигация на страницу создания претензии (Claim -> Assign Claim)
    /// </summary>
    private async Task NavigateToAssignClaimPage(IWebDriver driver)
    {
        // Клик на меню Claim
        var claimMenuLocator = By.XPath("//span[text()='Claim']/parent::a");
        WebAutomationHelper.WaitAndClick(driver, claimMenuLocator);

        await Task.Delay(1000);

        // Клик на Assign Claim
        var assignClaimLocator = By.XPath("//a[text()='Assign Claim']");
        WebAutomationHelper.WaitAndClick(driver, assignClaimLocator);

        await Task.Delay(1500);
    }

    /// <summary>
    /// Выбор сотрудника из автокомплита по имени
    /// Критично: просто ввести имя недостаточно, нужно выбрать из выпадающего списка!
    /// </summary>
    private bool SelectEmployeeFromAutocomplete(IWebDriver driver, string employeeName)
    {
        try
        {
            // Локатор поля Employee Name (autocomplete input)
            var employeeNameLocator = By.XPath("//label[text()='Employee Name']/ancestor::div[contains(@class,'oxd-input-group')]//input");

            // Используем helper для работы с автокомплитом
            bool success = WebAutomationHelper.TrySelectFromAutocomplete(driver, employeeNameLocator, employeeName);

            if (!success)
            {
                _logger.LogWarning("Не удалось выбрать сотрудника '{EmployeeName}' из автокомплита", employeeName);
            }

            return success;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при выборе сотрудника из автокомплита");
            return false;
        }
    }

    /// <summary>
    /// Валидация значения в dropdown (Event или Currency)
    /// </summary>
    private bool ValidateDropdownValue(IWebDriver driver, string fieldLabel, string expectedValue)
    {
        try
        {
            // Локатор dropdown по label
            var dropdownLocator = By.XPath($"//label[text()='{fieldLabel}']/ancestor::div[contains(@class,'oxd-input-group')]//div[@class='oxd-select-text-input']");

            // Получаем все опции из dropdown
            var options = WebAutomationHelper.GetDropdownOptions(driver, dropdownLocator);

            // Проверяем наличие значения
            bool found = options.Any(o => o.Equals(expectedValue, StringComparison.OrdinalIgnoreCase));

            if (!found)
            {
                _logger.LogWarning("{FieldLabel}: значение '{ExpectedValue}' не найдено. Доступные: {Options}",
                    fieldLabel, expectedValue, string.Join(", ", options));
            }

            return found;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при валидации поля {FieldLabel}", fieldLabel);
            return false;
        }
    }

    /// <summary>
    /// Заполнение полей претензии (Event, Currency, Remarks)
    /// </summary>
    private void FillClaimDetails(IWebDriver driver, ClaimRequest request)
    {
        // Event
        SelectDropdownValue(driver, "Event", request.Event);

        // Currency
        SelectDropdownValue(driver, "Currency", request.Currency);

        // Remarks (необязательное поле)
        if (!string.IsNullOrEmpty(request.Remarks))
        {
            var remarksLocator = By.XPath("//label[text()='Remarks']/ancestor::div[contains(@class,'oxd-input-group')]//textarea");
            WebAutomationHelper.WaitAndSendKeys(driver, remarksLocator, request.Remarks);
        }
    }

    /// <summary>
    /// Выбор значения из dropdown
    /// </summary>
    private void SelectDropdownValue(IWebDriver driver, string fieldLabel, string value)
    {
        var dropdownLocator = By.XPath($"//label[text()='{fieldLabel}']/ancestor::div[contains(@class,'oxd-input-group')]//div[@class='oxd-select-text-input']");

        bool success = WebAutomationHelper.TrySelectFromDropdown(driver, dropdownLocator, value);

        if (!success)
        {
            _logger.LogWarning("Не удалось выбрать '{Value}' в поле '{FieldLabel}'", value, fieldLabel);
        }
    }

    private void ClickCreateButton(IWebDriver driver)
    {
        var createButtonLocator = By.CssSelector("button[type='submit']");
        WebAutomationHelper.WaitAndClick(driver, createButtonLocator);
    }

    /// <summary>
    /// Парсинг Reference Id со страницы после создания претензии
    /// </summary>
    private string? ParseReferenceId(IWebDriver driver)
    {
        try
        {
            // Ожидаем загрузки страницы с деталями претензии
            Thread.Sleep(3000);

            // Вариант 1: Попробуем извлечь ID из URL (часто Reference ID = ID претензии в URL)
            var currentUrl = driver.Url;

            // URL формата: .../claim/assignClaim/id/12
            var urlMatch = System.Text.RegularExpressions.Regex.Match(currentUrl, @"/id/(\d+)");

            if (urlMatch.Success)
            {
                var idFromUrl = urlMatch.Groups[1].Value;
                return idFromUrl;
            }

            // Вариант 2: Попробуем найти на странице
            var selectors = new[]
            {
                By.XPath("//label[text()='Reference Id']/following-sibling::*"),
                By.XPath("//label[contains(text(), 'Reference')]/ancestor::div[contains(@class,'oxd-input-group')]//div[contains(@class,'oxd-input-group__label-wrapper')]/following-sibling::div"),
                By.XPath("//label[contains(text(), 'Reference')]/parent::div/following-sibling::div"),
                By.XPath("//div[contains(@class,'oxd-form')]//label[contains(text(), 'Reference')]/parent::*/following-sibling::*")
            };

            foreach (var selector in selectors)
            {
                try
                {
                    var elements = driver.FindElements(selector);

                    foreach (var element in elements)
                    {
                        var text = element.Text.Trim();
                        
                        if (!string.IsNullOrEmpty(text) && text.Length > 0 && text != "Reference Id")
                        {
                            return text;
                        }
                    }
                }
                catch (NoSuchElementException)
                {
                    continue;
                }
            }

            _logger.LogWarning("Не удалось найти Reference ID ни в URL, ни на странице");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при парсинге Reference ID");
            return null;
        }
    }
}
