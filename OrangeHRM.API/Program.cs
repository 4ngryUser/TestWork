using Microsoft.EntityFrameworkCore;
using OrangeHRM.API.Configuration;
using OrangeHRM.API.Data;
using OrangeHRM.API.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Настройка Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .CreateLogger();

builder.Host.UseSerilog();

// Регистрация конфигурационных классов (Options pattern)
builder.Services.Configure<OrangeHRMSettings>(
    builder.Configuration.GetSection(OrangeHRMSettings.SectionName));

builder.Services.Configure<WebDriverSettings>(
    builder.Configuration.GetSection(WebDriverSettings.SectionName));

// Регистрация DbContext (SQLite)
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Регистрация репозиториев
builder.Services.AddScoped<IEmployeeRepository, EmployeeRepository>();

// Регистрация сервисов
builder.Services.AddSingleton<IWebDriverFactory, ChromeDriverFactory>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IClaimService, ClaimService>();

// Регистрация контроллеров
builder.Services.AddControllers();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new()
    {
        Title = "OrangeHRM API",
        Version = "v1",
        Description = "API для автоматизации работы с OrangeHRM Demo (добавление сотрудников и создание претензий)"
    });
});

var app = builder.Build();

// Автоматическое применение миграций при старте (создание БД если не существует)
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    dbContext.Database.EnsureCreated();
    Log.Information("База данных инициализирована");
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "OrangeHRM API v1");
    });
}

// Middleware
app.UseSerilogRequestLogging();
app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

Log.Information("OrangeHRM API запущен");

try
{
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Приложение завершилось с ошибкой");
}
finally
{
    Log.CloseAndFlush();
}