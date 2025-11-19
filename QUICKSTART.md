# Быстрый старт OrangeHRM API

## Запуск проекта в VS Code

### Через меню Debug (F5)

1. Открой папку с проектом в VS Code (корневую папку с файлами OrangeHRM.API, QueryDb, test_employee.json)
2. Нажми **F5** или перейди в панель **Run and Debug** (Ctrl+Shift+D)
3. Выбери конфигурацию **"Launch OrangeHRM API"**
4. Нажми зеленую кнопку **Start Debugging** (или F5)

Проект автоматически:
- Соберется (build)
- Запустится на http://localhost:5167

Проект запустится на http://localhost:5167

## Тестирование API

1. **Запусти сервер через F5** - нажми **F5** в VS Code
2. **Дождись запуска** - увидишь в консоли Debug: `OrangeHRM API запущен`
3. **Открой терминал PowerShell** - нажми **Ctrl+Shift+`** (создаст новый терминал)
4. **Выполни команды тестирования** в терминале:

```powershell
# Создать сотрудника
Invoke-RestMethod -Uri "http://localhost:5167/api/OrangeHRM/employee" -Method POST -ContentType "application/json; charset=utf-8" -InFile "test_employee.json"

# Создать претензию
Invoke-RestMethod -Uri "http://localhost:5167/api/OrangeHRM/claim" -Method POST -ContentType "application/json; charset=utf-8" -InFile "test_claim.json"

# Посмотреть БД
cd QueryDb
dotnet run
```

## Просмотр логов

Логи сохраняются в папку `...\OrangeHRM.API\logs\`

**Формат имени:** `log-YYYYMMDD.txt`

Пример: `log-20251116.txt`

## Горячие клавиши VS Code

## Структура проекта

```
<корень проекта>/
├── OrangeHRM.API/          # Главный проект
│   ├── Controllers/        # API endpoints
│   ├── Services/           # Бизнес-логика
│   ├── Helpers/            # Selenium автоматизация
│   ├── Data/               # База данных
│   ├── Models/             # Модели запросов/ответов
│   ├── appsettings.json    # Настройки (URL, логин, пароль)
│   └── orangehrm.db        # SQLite база данных
├── QueryDb/                # Утилита просмотра БД
├── test_employee.json      # Тестовые данные сотрудника
├── test_claim.json         # Тестовые данные претензии
└── .vscode/                # Конфигурация VS Code
    ├── launch.json         # Настройки запуска
    └── tasks.json          # Задачи сборки
```

## Настройки проекта

Файл: `...\OrangeHRM.API\appsettings.json`

```json
{
  "OrangeHRM": {
    "BaseUrl": "https://opensource-demo.orangehrmlive.com/",
    "Username": "Admin",
    "Password": "admin123"
  },
  "WebDriver": {
    "Headless": false,        // true = браузер не показывается
    "ImplicitWaitSeconds": 10,
    "PageLoadTimeoutSeconds": 30,
    "CommandTimeoutSeconds": 60
  }
}
```

## Частые проблемы

### Порт занят
```
Error: Failed to bind to address http://127.0.0.1:5167
```

**Решение:**
```bash
netstat -ano | findstr :5167
taskkill /F /PID <номер_процесса>
```

### ChromeDriver не найден
```
Error: Unable to locate or obtain driver
```

**Решение:**
Пакет автоматически скачает нужную версию ChromeDriver при первом запуске.

### Браузер не открывается в headless режиме
Установи в `appsettings.json`:
```json
"Headless": false
```

Чтобы видеть что происходит в браузере.
