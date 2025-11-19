using OpenQA.Selenium;
using OrangeHRM.API.Data;
using OrangeHRM.API.Helpers;
using OrangeHRM.API.Models;

namespace OrangeHRM.API.Services;

/// <summary>
/// Сервис для работы с сотрудниками в OrangeHRM
/// </summary>
public class EmployeeService : IEmployeeService
{
    private readonly IWebDriverFactory _driverFactory;
    private readonly IAuthService _authService;
    private readonly IEmployeeRepository _employeeRepository;
    private readonly ILogger<EmployeeService> _logger;

    public EmployeeService(
        IWebDriverFactory driverFactory,
        IAuthService authService,
        IEmployeeRepository employeeRepository,
        ILogger<EmployeeService> logger)
    {
        _driverFactory = driverFactory;
        _authService = authService;
        _employeeRepository = employeeRepository;
        _logger = logger;
    }

    /// <summary>
    /// Добавить нового сотрудника в систему OrangeHRM
    /// </summary>
    public async Task<EmployeeResponse> AddEmployeeAsync(EmployeeRequest request)
    {
        IWebDriver? driver = null;

        try
        {
            _logger.LogInformation("Начало добавления сотрудника: {FirstName} {MiddleName} {LastName}",
                request.FirstName, request.MiddleName, request.LastName);

            // Создание WebDriver
            driver = _driverFactory.CreateDriver();

            // Шаг 1: Авторизация
            var loginSuccess = await _authService.LoginAsync(driver);
            if (!loginSuccess)
            {
                return EmployeeResponse.CreateError("Не удалось авторизоваться в системе");
            }

            // Шаг 2: Навигация PIM -> Add Employee
            await NavigateToAddEmployeePage(driver);

            // Шаг 3: Генерация уникального Employee ID
            var employeeId = GenerateUniqueEmployeeId(driver);
            if (employeeId == null)
            {
                return EmployeeResponse.CreateError("Не удалось сгенерировать уникальный Employee ID");
            }

            // Шаг 4: Заполнение полей First/Middle/Last Name и Employee ID
            FillEmployeeBasicInfo(driver, request, employeeId);

            // Шаг 5: Сохранение базовой информации сотрудника
            ClickSaveButton(driver);

            // Ожидание перехода на страницу профиля сотрудника
            await Task.Delay(3000);

            // Шаг 6: Переход на вкладку Job (на странице профиля)
            NavigateToJobTab(driver);

            // Шаг 7: Заполнение Job полей (валидация происходит во время заполнения)
            FillJobDetails(driver, request.Job);

            // Шаг 8: Сохранение Job Details
            ClickSaveButton(driver);

            // Ожидание сохранения
            await Task.Delay(1500);

            _logger.LogInformation("Сотрудник успешно добавлен с ID: {EmployeeId}", employeeId);

            // Шаг 10: Сохранение в БД (дополнительное задание)
            await SaveEmployeeToDatabase(request, employeeId);

            // Шаг 11: Обновление test_claim.json с новым employeeId
            await UpdateTestClaimFile(employeeId);

            return EmployeeResponse.CreateSuccess(employeeId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при добавлении сотрудника");
            return EmployeeResponse.CreateError($"Ошибка при добавлении сотрудника: {ex.Message}");
        }
        finally
        {
            driver?.Quit();
            driver?.Dispose();
        }
    }

    /// <summary>
    /// Навигация на страницу добавления сотрудника (PIM -> Add Employee)
    /// </summary>
    private async Task NavigateToAddEmployeePage(IWebDriver driver)
    {
        // Клик на меню PIM
        var pimMenuLocator = By.XPath("//span[text()='PIM']/parent::a");
        WebAutomationHelper.WaitAndClick(driver, pimMenuLocator);

        await Task.Delay(1000);

        // Клик на Add Employee
        var addEmployeeLocator = By.XPath("//a[text()='Add Employee']");
        WebAutomationHelper.WaitAndClick(driver, addEmployeeLocator);

        await Task.Delay(1500);
    }

    /// <summary>
    /// Генерация уникального Employee ID с проверкой на странице
    /// </summary>
    private string? GenerateUniqueEmployeeId(IWebDriver driver)
    {
        var employeeIdLocator = By.XPath("//label[text()='Employee Id']/ancestor::div[contains(@class,'oxd-input-group')]//input");

        return EmployeeIdGenerator.GenerateUnique(id =>
        {
            // Вводим сгенерированный ID
            var field = WebAutomationHelper.WaitForElement(driver, employeeIdLocator);

            // Полная очистка поля: выделить все и удалить
            field.Click();
            field.SendKeys(Keys.Control + "a");
            field.SendKeys(Keys.Delete);

            // Ввод нового ID
            field.SendKeys(id);

            // Даем время на валидацию
            Thread.Sleep(500);

            // Проверяем наличие ошибки (красная подсветка или сообщение)
            bool hasError = WebAutomationHelper.HasFieldError(driver, employeeIdLocator);

            if (hasError)
            {
                return false;
            }

            return true;
        }, maxAttempts: 10);
    }

    /// <summary>
    /// Заполнение базовой информации о сотруднике
    /// </summary>
    private void FillEmployeeBasicInfo(IWebDriver driver, EmployeeRequest request, string employeeId)
    {
        // First Name
        var firstNameLocator = By.Name("firstName");
        WebAutomationHelper.WaitAndSendKeys(driver, firstNameLocator, request.FirstName);

        // Middle Name
        var middleNameLocator = By.Name("middleName");
        WebAutomationHelper.WaitAndSendKeys(driver, middleNameLocator, request.MiddleName);

        // Last Name
        var lastNameLocator = By.Name("lastName");
        WebAutomationHelper.WaitAndSendKeys(driver, lastNameLocator, request.LastName);
    }

    private void ClickSaveButton(IWebDriver driver)
    {
        var saveButtonLocator = By.CssSelector("button[type='submit']");
        WebAutomationHelper.WaitAndClick(driver, saveButtonLocator);
    }

    /// <summary>
    /// Переход на вкладку Job
    /// </summary>
    private void NavigateToJobTab(IWebDriver driver)
    {
        var possibleSelectors = new[]
        {
            By.XPath("//a[contains(translate(text(), 'JOB', 'job'), 'job')]"),
            By.XPath("//*[contains(@class, 'nav')]//a[contains(text(), 'Job')]"),
            By.XPath("//div[contains(@class, 'orangehrm-tabs')]//a[contains(text(), 'Job')]"),
            By.CssSelector("a[href*='job' i], a[href*='Job']"),
            By.XPath("//*[@role='tab' and contains(translate(., 'JOB', 'job'), 'job')]")
        };

        IWebElement? jobTab = null;
        foreach (var selector in possibleSelectors)
        {
            try
            {
                jobTab = driver.FindElement(selector);
                if (jobTab != null && jobTab.Displayed)
                {
                    jobTab.Click();
                    Thread.Sleep(1500);
                    return;
                }
            }
            catch (NoSuchElementException)
            {
                // Продолжаем поиск
            }
        }
    }


    /// <summary>
    /// Заполнение Job Details
    /// </summary>
    private void FillJobDetails(IWebDriver driver, JobDetails job)
    {
        // Joined Date (текущая дата)
        var joinedDateLocator = By.XPath("//label[text()='Joined Date']/ancestor::div[contains(@class,'oxd-input-group')]//input");
        var currentDate = DateTime.Now.ToString("yyyy-MM-dd");
        WebAutomationHelper.WaitAndSendKeys(driver, joinedDateLocator, currentDate);

        // Job Title
        SelectDropdownValue(driver, "Job Title", job.JobTitle);

        // Job Category
        SelectDropdownValue(driver, "Job Category", job.JobCategory);

        // Sub Unit
        SelectDropdownValue(driver, "Sub Unit", job.SubUnit);

        // Location
        SelectDropdownValue(driver, "Location", job.Location);

        // Employment Status
        SelectDropdownValue(driver, "Employment Status", job.EmploymentStatus);
    }

    /// <summary>
    /// Выбор значения из dropdown
    /// </summary>
    private void SelectDropdownValue(IWebDriver driver, string fieldLabel, string value)
    {
        _logger.LogDebug("Попытка выбрать значение '{Value}' в поле '{FieldLabel}'", value, fieldLabel);

        var dropdownLocator = By.XPath($"//label[text()='{fieldLabel}']/ancestor::div[contains(@class,'oxd-input-group')]//div[@class='oxd-select-text-input']");

        bool success = WebAutomationHelper.TrySelectFromDropdown(driver, dropdownLocator, value);

        if (!success)
        {
            var errorMessage = $"Не удалось выбрать '{value}' в поле '{fieldLabel}'";
            _logger.LogError(errorMessage);
            throw new Exception(errorMessage);
        }

        _logger.LogDebug("Значение '{Value}' успешно выбрано в поле '{FieldLabel}'", value, fieldLabel);
    }

    /// <summary>
    /// Сохранить данные сотрудника в БД
    /// </summary>
    private async Task SaveEmployeeToDatabase(EmployeeRequest request, string employeeId)
    {
        try
        {
            var employee = new Employee
            {
                FirstName = request.FirstName,
                MiddleName = request.MiddleName,
                LastName = request.LastName,
                EmployeeId = employeeId
            };

            await _employeeRepository.SaveEmployeeAsync(employee);

            _logger.LogInformation("Данные сотрудника сохранены в БД: {EmployeeId}", employeeId);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при сохранении сотрудника в БД: {EmployeeId}", employeeId);
        }
    }

    /// <summary>
    /// Обновление файла test_claim.json с новым employeeId
    /// </summary>
    private async Task UpdateTestClaimFile(string employeeId)
    {
        try
        {
            var testClaimPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "test_claim.json");

            if (File.Exists(testClaimPath))
            {
                var claimJson = await File.ReadAllTextAsync(testClaimPath);
                var claimData = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.JsonElement>(claimJson);

                var updatedClaim = new
                {
                    employeeId = employeeId,
                    @event = claimData.TryGetProperty("event", out var eventProp) ? eventProp.GetString() : "Accommodation",
                    currency = claimData.TryGetProperty("currency", out var currencyProp) ? currencyProp.GetString() : "Russian Rouble",
                    remarks = $"Test claim for employee {employeeId}"
                };

                var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
                var updatedJson = System.Text.Json.JsonSerializer.Serialize(updatedClaim, options);

                await File.WriteAllTextAsync(testClaimPath, updatedJson);

                _logger.LogInformation("Файл test_claim.json обновлен с employeeId: {EmployeeId}", employeeId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Не удалось обновить test_claim.json");
        }
    }
}