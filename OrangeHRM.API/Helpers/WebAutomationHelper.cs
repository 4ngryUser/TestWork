using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;

namespace OrangeHRM.API.Helpers;

/// <summary>
/// Вспомогательные методы для работы с Selenium WebDriver
/// </summary>
public static class WebAutomationHelper
{
    /// <summary>
    /// Ожидать появления элемента и вернуть его
    /// </summary>
    public static IWebElement WaitForElement(IWebDriver driver, By locator, int timeoutSeconds = 10)
    {
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutSeconds));
        return wait.Until(d =>
        {
            var element = d.FindElement(locator);
            return element.Displayed ? element : null;
        });
    }

    /// <summary>
    /// Ожидать кликабельности элемента и кликнуть
    /// </summary>
    public static void WaitAndClick(IWebDriver driver, By locator, int timeoutSeconds = 10)
    {
        var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(timeoutSeconds));
        var element = wait.Until(d =>
        {
            try
            {
                var el = d.FindElement(locator);
                return el.Displayed && el.Enabled ? el : null;
            }
            catch (StaleElementReferenceException)
            {
                return null;
            }
        });
        element!.Click();
    }

    /// <summary>
    /// Ожидать элемент, очистить и ввести текст
    /// </summary>
    public static void WaitAndSendKeys(IWebDriver driver, By locator, string text, int timeoutSeconds = 10)
    {
        var element = WaitForElement(driver, locator, timeoutSeconds);
        element.Clear();
        element.SendKeys(text);
    }

    /// <summary>
    /// Получить все опции из dropdown
    /// </summary>
    public static List<string> GetDropdownOptions(IWebDriver driver, By dropdownLocator)
    {
        var dropdown = WaitForElement(driver, dropdownLocator);
        dropdown.Click(); // Открыть dropdown

        // Подождать появления опций
        Thread.Sleep(500);

        var options = driver.FindElements(By.CssSelector("div[role='option'], option"));
        var optionTexts = options.Select(o => o.Text.Trim()).Where(t => !string.IsNullOrEmpty(t)).ToList();

        // Закрыть dropdown (кликнуть Escape)
        dropdown.SendKeys(Keys.Escape);

        return optionTexts;
    }

    /// <summary>
    /// Выбрать значение из dropdown по тексту
    /// </summary>
    public static bool TrySelectFromDropdown(IWebDriver driver, By dropdownLocator, string value)
    {
        var dropdown = WaitForElement(driver, dropdownLocator);

        // Скроллим к элементу перед кликом
        ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", dropdown);
        Thread.Sleep(300);

        dropdown.Click();
        Thread.Sleep(1500); // Увеличено время ожидания появления списка

        // Попробуем несколько вариантов селекторов для опций
        var selectors = new[]
        {
            By.XPath($"//*[@role='option' and normalize-space(text())='{value}']"),
            By.XPath($"//div[@role='option']/span[normalize-space(text())='{value}']"),
            By.XPath($"//div[@role='listbox']//span[normalize-space(text())='{value}']"),
            By.XPath($"//div[contains(@class, 'oxd-select-option')]/span[normalize-space(text())='{value}']"),
            By.XPath($"//div[contains(@class, 'oxd-select-dropdown')]//span[normalize-space(text())='{value}']")
        };

        foreach (var selector in selectors)
        {
            try
            {
                var options = driver.FindElements(selector);
                foreach (var option in options)
                {
                    if (option.Displayed && option.Enabled && option.Text.Trim().Equals(value, StringComparison.OrdinalIgnoreCase))
                    {
                        // Скроллим к опции перед кликом
                        ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", option);
                        Thread.Sleep(200);

                        option.Click();
                        Thread.Sleep(500);
                        return true;
                    }
                }
            }
            catch (NoSuchElementException)
            {
                // Продолжаем со следующим селектором
            }
            catch (StaleElementReferenceException)
            {
                // Элемент устарел, пробуем еще раз
                Thread.Sleep(300);
            }
        }

        // Если ничего не сработало, закрыть dropdown
        try
        {
            dropdown.SendKeys(Keys.Escape);
        }
        catch { }

        return false;
    }

    /// <summary>
    /// Работа с autocomplete: ввод текста и выбор из предложенных вариантов
    /// Пробует несколько вариантов поиска если первый не сработал
    /// </summary>
    public static bool TrySelectFromAutocomplete(IWebDriver driver, By inputLocator, string searchText)
    {
        // Генерируем варианты поиска
        var searchVariants = GenerateSearchVariants(searchText);

        foreach (var variant in searchVariants)
        {
            if (TrySelectFromAutocompleteInternal(driver, inputLocator, variant))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Генерация вариантов поиска для ФИО
    /// Порядок поиска: Фамилия -> Имя Фамилия -> Полное ФИО
    /// Пример: "Иван Петрович Сидоров" -> ["Сидоров", "Иван Сидоров", "Иван Петрович Сидоров"]
    /// </summary>
    private static List<string> GenerateSearchVariants(string fullName)
    {
        var variants = new List<string>();
        var parts = fullName.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (parts.Length == 3)
        {
            // Полное имя: Имя Отчество Фамилия
            variants.Add(parts[2]); // 1. Только фамилия
            variants.Add($"{parts[0]} {parts[2]}"); // 2. Имя + Фамилия (без отчества)
            variants.Add(fullName); // 3. Полное ФИО
        }
        else if (parts.Length == 2)
        {
            // Имя Фамилия
            variants.Add(parts[1]); // 1. Только фамилия
            variants.Add(fullName); // 2. Имя + Фамилия
        }
        else
        {
            // Одно слово или что-то еще
            variants.Add(fullName);
        }

        return variants.Distinct().ToList();
    }

    /// <summary>
    /// Внутренний метод для работы с автокомплитом (одна попытка)
    /// </summary>
    private static bool TrySelectFromAutocompleteInternal(IWebDriver driver, By inputLocator, string searchText)
    {
        try
        {
            var input = WaitForElement(driver, inputLocator);

            // Полная очистка поля: выделить все и удалить
            input.Click();
            input.SendKeys(Keys.Control + "a");
            input.SendKeys(Keys.Delete);
            Thread.Sleep(300);

            // Ввод текста
            input.SendKeys(searchText);

            Thread.Sleep(3000);

            // Пробуем несколько вариантов селекторов для автокомплита
            var suggestionSelectors = new[]
            {
                // Поиск по частичному совпадению текста
                By.XPath($"//*[contains(@class, 'autocomplete')]//div[contains(text(), '{searchText}')]"),
                By.XPath($"//*[@role='option' and contains(., '{searchText}')]"),
                By.XPath($"//*[@role='listbox']//div[contains(., '{searchText}')]"),
                By.XPath($"//div[contains(@class, 'oxd-autocomplete-option')]"),
                By.XPath($"//div[contains(@class, 'autocomplete-dropdown')]//div[contains(@class, 'option')]"),
                // Попробуем найти любой элемент с role='option'
                By.XPath("//*[@role='option']")
            };

            var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(10));

            foreach (var selector in suggestionSelectors)
            {
                try
                {
                    var suggestions = driver.FindElements(selector);

                    // Ищем элемент, содержащий наш текст
                    foreach (var suggestion in suggestions)
                    {
                        if (suggestion.Displayed && suggestion.Text.Contains(searchText, StringComparison.OrdinalIgnoreCase))
                        {
                            suggestion.Click();
                            Thread.Sleep(500); // Даем время на выбор
                            return true;
                        }
                    }
                }
                catch (NoSuchElementException)
                {
                    // Пробуем следующий селектор
                    continue;
                }
                catch (StaleElementReferenceException)
                {
                    // Элемент устарел, пробуем следующий
                    continue;
                }
            }

            return false;
        }
        catch (WebDriverTimeoutException)
        {
            return false;
        }
        catch (StaleElementReferenceException)
        {
            return false;
        }
    }

    /// <summary>
    /// Проверить наличие ошибки в поле (красная подсветка или сообщение об ошибке)
    /// </summary>
    public static bool HasFieldError(IWebDriver driver, By fieldLocator)
    {
        try
        {
            var field = driver.FindElement(fieldLocator);
            var parentClasses = field.GetAttribute("class");

            // Проверка на классы ошибок (обычно содержат 'error', 'invalid' и т.д.)
            if (parentClasses != null && (parentClasses.Contains("error") || parentClasses.Contains("invalid")))
            {
                return true;
            }

            // Проверка на сообщение об ошибке рядом с полем (используя IWebElement вместо строки локатора)
            try
            {
                var errorMessage = field.FindElement(By.XPath("..//span[contains(@class, 'error')]"));
                return errorMessage.Displayed;
            }
            catch (NoSuchElementException)
            {
                return false;
            }
        }
        catch (NoSuchElementException)
        {
            return false;
        }
    }
}
