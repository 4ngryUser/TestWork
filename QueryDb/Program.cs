using Microsoft.Data.Sqlite;

var dbPath = Path.Combine(Directory.GetCurrentDirectory(), "..", "OrangeHRM.API", "orangehrm.db");
var connectionString = $"Data Source={dbPath}";

using (var connection = new SqliteConnection(connectionString))
{
    connection.Open();

    var command = connection.CreateCommand();
    command.CommandText = "SELECT * FROM Employees ORDER BY CreatedAt DESC";

    using var reader = command.ExecuteReader();

    Console.WriteLine("=== Все сотрудники в базе данных ===\n");
    Console.WriteLine($"{"ID",-5} {"EmployeeId",-12} {"First Name",-15} {"Middle Name",-15} {"Last Name",-15} {"Created At",-20}");
    Console.WriteLine(new string('-', 100));

    int count = 0;
    while (reader.Read())
    {
        count++;
        var id = reader.GetInt32(0);
        var firstName = reader.GetString(1);
        var middleName = reader.GetString(2);
        var lastName = reader.GetString(3);
        var employeeId = reader.GetString(4);
        var createdAt = reader.GetDateTime(5).ToString("yyyy-MM-dd HH:mm:ss");

        Console.WriteLine($"{id,-5} {employeeId,-12} {firstName,-15} {middleName,-15} {lastName,-15} {createdAt,-20}");
    }

    Console.WriteLine(new string('-', 100));
    Console.WriteLine($"\nВсего записей: {count}");
}
