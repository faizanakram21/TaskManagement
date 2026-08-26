using FluentAssertions;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;
using TaskManagement.Application.Services;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Interfaces;
using Xunit;

namespace TaskManagement.Tests.Unit.Services;

public class TaskServiceTests
{
    private readonly Mock<IUnitOfWork> _uowMock = new();
    private readonly Mock<ITaskRepository> _tasksMock = new();
    private readonly IMemoryCache _cache = new MemoryCache(new MemoryCacheOptions());
    private readonly TaskService _sut;

    public TaskServiceTests()
    {
        _uowMock.Setup(u => u.Tasks).Returns(_tasksMock.Object);
        _sut = new TaskService(_uowMock.Object, _cache);
    }

    [Fact]
    public async Task CreateAsync_WithValidData_CreatesTaskForCorrectUser()
    {
        var dto = new CreateTaskDto("Buy groceries", "Milk, eggs, bread");

        var result = await _sut.CreateAsync(userId: 42, dto);

        result.Title.Should().Be("Buy groceries");
        result.UserId.Should().Be(42);
        result.IsCompleted.Should().BeFalse();
        _tasksMock.Verify(r => r.AddAsync(It.Is<TaskItem>(t => t.UserId == 42 && t.Title == "Buy groceries")), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_WhenTaskDoesNotExist_ThrowsKeyNotFoundException()
    {
        _tasksMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((TaskItem?)null);

        var act = () => _sut.UpdateAsync(userId: 1, taskId: 999, new UpdateTaskDto("x", "y", true));

        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    [Fact]
    public async Task UpdateAsync_WhenTaskBelongsToAnotherUser_ThrowsUnauthorizedAccessException()
    {
        var task = new TaskItem { Id = 5, UserId = 100, Title = "Someone else's task" };
        _tasksMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(task);

        var act = () => _sut.UpdateAsync(userId: 1, taskId: 5, new UpdateTaskDto("Hacked title", "desc", true));

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        _tasksMock.Verify(r => r.Update(It.IsAny<TaskItem>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenOwnedByCorrectUser_UpdatesFieldsAndSaves()
    {
        var task = new TaskItem { Id = 5, UserId = 1, Title = "Old title", Description = "Old desc", IsCompleted = false };
        _tasksMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(task);

        var result = await _sut.UpdateAsync(userId: 1, taskId: 5, new UpdateTaskDto("New title", "New desc", true));

        result.Title.Should().Be("New title");
        result.IsCompleted.Should().BeTrue();
        _tasksMock.Verify(r => r.Update(It.Is<TaskItem>(t => t.Title == "New title")), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenTaskBelongsToAnotherUser_ThrowsUnauthorizedAccessException()
    {
        var task = new TaskItem { Id = 5, UserId = 100 };
        _tasksMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(task);

        var act = () => _sut.DeleteAsync(userId: 1, taskId: 5);

        await act.Should().ThrowAsync<UnauthorizedAccessException>();
        _tasksMock.Verify(r => r.Delete(It.IsAny<TaskItem>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WhenOwnedByCorrectUser_DeletesAndSaves()
    {
        var task = new TaskItem { Id = 5, UserId = 1 };
        _tasksMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(task);

        await _sut.DeleteAsync(userId: 1, taskId: 5);

        _tasksMock.Verify(r => r.Delete(task), Times.Once);
        _uowMock.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task ToggleCompleteAsync_FlipsIsCompletedValue()
    {
        var task = new TaskItem { Id = 5, UserId = 1, IsCompleted = false };
        _tasksMock.Setup(r => r.GetByIdAsync(5)).ReturnsAsync(task);

        var result = await _sut.ToggleCompleteAsync(userId: 1, taskId: 5);

        result.IsCompleted.Should().BeTrue();
    }

    [Fact]
    public async Task GetMyTasksAsync_CachesResultOnSecondCall()
    {
        var tasks = new List<TaskItem> { new() { Id = 1, UserId = 7, Title = "A" } };
        _tasksMock.Setup(r => r.GetByUserIdAsync(7)).ReturnsAsync(tasks);

        await _sut.GetMyTasksAsync(7);   // first call -> hits repository, populates cache
        await _sut.GetMyTasksAsync(7);   // second call -> should come from cache

        _tasksMock.Verify(r => r.GetByUserIdAsync(7), Times.Once,
            "the second call should be served from cache instead of hitting the repository again");
    }

    [Fact]
    public async Task CreateAsync_InvalidatesCacheForThatUser()
    {
        var tasks = new List<TaskItem> { new() { Id = 1, UserId = 7, Title = "A" } };
        _tasksMock.Setup(r => r.GetByUserIdAsync(7)).ReturnsAsync(tasks);

        await _sut.GetMyTasksAsync(7); // populates cache
        await _sut.CreateAsync(7, new CreateTaskDto("New task", "desc")); // should clear cache
        await _sut.GetMyTasksAsync(7); // should hit repository again

        _tasksMock.Verify(r => r.GetByUserIdAsync(7), Times.Exactly(2));
    }
}