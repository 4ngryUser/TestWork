using Microsoft.EntityFrameworkCore;

namespace OrangeHRM.API.Data;

/// <summary>
/// Контекст базы данных для работы с сотрудниками
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    /// <summary>
    /// Таблица сотрудников
    /// </summary>
    public DbSet<Employee> Employees { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Настройка индекса на EmployeeId для быстрого поиска
        modelBuilder.Entity<Employee>()
            .HasIndex(e => e.EmployeeId)
            .IsUnique();
    }
}
