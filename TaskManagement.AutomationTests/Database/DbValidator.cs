using Dapper;
using Microsoft.Data.SqlClient;
using TaskManagement.AutomationTests.Config;

namespace TaskManagement.AutomationTests.Database;

public record TaskRow(int Id, string Title, string? Description, bool IsCompleted, DateTime CreatedAt, int UserId);
public record UserRow(int Id, string Name, string Email, string Role);

public class DbValidator
{
    private readonly string _connectionString = TestSettings.Instance.DbConnectionString;

    public TaskRow? GetTaskById(int taskId)
    {
        using var connection = new SqlConnection(_connectionString);
        const string sql = """
            SELECT Id, Title, Description, IsCompleted, CreatedAt, UserId
            FROM Tasks
            WHERE Id = @TaskId
            """;
        return connection.QuerySingleOrDefault<TaskRow>(sql, new { TaskId = taskId });
    }

    public UserRow? GetUserByEmail(string email)
    {
        using var connection = new SqlConnection(_connectionString);
        const string sql = """
            SELECT Id, Name, Email, Role
            FROM Users
            WHERE Email = @Email
            """;
        return connection.QuerySingleOrDefault<UserRow>(sql, new { Email = email });
    }
}