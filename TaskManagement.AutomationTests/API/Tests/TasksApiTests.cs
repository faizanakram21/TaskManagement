using System.Net;
using FluentAssertions;
using TaskManagement.AutomationTests.API.Clients;
using TaskManagement.AutomationTests.Database;
using TaskManagement.AutomationTests.Helpers;

namespace TaskManagement.AutomationTests.API.Tests;

[TestFixture]
[Category("API")]
public class TasksApiTests
{
    private AuthApiClient _authClient = null!;
    private TasksApiClient _tasksClient = null!;
    private string _token = null!;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        _authClient = new AuthApiClient();
        _token = _authClient.GetValidToken();
    }

    [SetUp]
    public void SetUp()
    {
        _tasksClient = new TasksApiClient();
    }

    [Test]
    public void GetAll_WithoutToken_ReturnsUnauthorized()
    {
        var response = _tasksClient.GetAll(token: "");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Test]
    public void Create_WithValidToken_CreatesTaskAndPersistsToDatabase()
    {
        var (title, description, dueDate) = TestDataBuilder.NewTask();
        var newTask = new TaskDto(null, title, description, false, dueDate);

        var response = _tasksClient.Create(newTask, _token);

        response.IsSuccessful.Should().BeTrue($"Body: {response.Content}");
        response.Data.Should().NotBeNull();
        response.Data!.Id.Should().NotBeNull();
        response.Data.Title.Should().Be(title);

        var dbValidator = new DbValidator();
        var persisted = dbValidator.GetTaskById(response.Data.Id!.Value);

        persisted.Should().NotBeNull("task created via API should exist in the database");
        persisted!.Title.Should().Be(title);
    }

    [Test]
    public void Update_ExistingTask_MarksAsCompleted()
    {
        var (title, description, dueDate) = TestDataBuilder.NewTask();
        var created = _tasksClient.Create(new TaskDto(null, title, description, false, dueDate), _token);
        var taskId = created.Data!.Id!.Value;

        var updated = created.Data with { IsCompleted = true };
        var response = _tasksClient.Update(taskId, updated, _token);

        response.IsSuccessful.Should().BeTrue();
        response.Data!.IsCompleted.Should().BeTrue();
    }

    [Test]
    public void Delete_ExistingTask_RemovesItFromList()
    {
        var (title, description, dueDate) = TestDataBuilder.NewTask();
        var created = _tasksClient.Create(new TaskDto(null, title, description, false, dueDate), _token);
        var taskId = created.Data!.Id!.Value;

        var deleteResponse = _tasksClient.Delete(taskId, _token);
        deleteResponse.IsSuccessful.Should().BeTrue();

        var allTasks = _tasksClient.GetAll(_token);
        allTasks.Data!.Should().NotContain(t => t.Id == taskId);
    }
}