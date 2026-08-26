using Microsoft.Extensions.Caching.Memory;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Services;

public class TaskService : ITaskService  // 👈 add this
{
    private readonly IUnitOfWork _uow;
    private readonly IMemoryCache _cache;

    private static string UserCacheKey(int userId) => $"tasks_user_{userId}";
    private const string AllTasksCacheKey = "tasks_all";

    public TaskService(IUnitOfWork uow, IMemoryCache cache)
    {
        _uow = uow;
        _cache = cache;
    }

    public async Task<IEnumerable<TaskResponseDto>> GetMyTasksAsync(int userId)
    {
        var cacheKey = UserCacheKey(userId);

        if (_cache.TryGetValue(cacheKey, out IEnumerable<TaskResponseDto>? cached))
            return cached!;

        var tasks = await _uow.Tasks.GetByUserIdAsync(userId);
        var result = tasks.Select(MapToDto);

        _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));
        return result;
    }

    public async Task<IEnumerable<TaskResponseDto>> GetAllTasksAsync()
    {
        if (_cache.TryGetValue(AllTasksCacheKey, out IEnumerable<TaskResponseDto>? cached))
            return cached!;

        var tasks = await _uow.Tasks.GetAllAsync();
        var result = tasks.Select(MapToDto);

        _cache.Set(AllTasksCacheKey, result, TimeSpan.FromMinutes(5));
        return result;
    }

    public async Task<TaskResponseDto> CreateAsync(int userId, CreateTaskDto dto)
    {
        var task = new TaskItem
        {
            Title = dto.Title,
            Description = dto.Description,
            UserId = userId
        };
        await _uow.Tasks.AddAsync(task);
        await _uow.SaveChangesAsync();

        _cache.Remove(UserCacheKey(userId));
        _cache.Remove(AllTasksCacheKey);

        return MapToDto(task);
    }

    public async Task<TaskResponseDto> UpdateAsync(int userId, int taskId, UpdateTaskDto dto)
    {
        var task = await _uow.Tasks.GetByIdAsync(taskId)
            ?? throw new KeyNotFoundException("Task not found.");

        if (task.UserId != userId)
            throw new UnauthorizedAccessException("Not your task.");

        task.Title = dto.Title;
        task.Description = dto.Description;
        task.IsCompleted = dto.IsCompleted;

        _uow.Tasks.Update(task);
        await _uow.SaveChangesAsync();

        _cache.Remove(UserCacheKey(userId));
        _cache.Remove(AllTasksCacheKey);

        return MapToDto(task);
    }

    public async Task DeleteAsync(int userId, int taskId)
    {
        var task = await _uow.Tasks.GetByIdAsync(taskId)
            ?? throw new KeyNotFoundException("Task not found.");

        if (task.UserId != userId)
            throw new UnauthorizedAccessException("Not your task.");

        _uow.Tasks.Delete(task);
        await _uow.SaveChangesAsync();

        _cache.Remove(UserCacheKey(userId));
        _cache.Remove(AllTasksCacheKey);
    }

    public async Task<TaskResponseDto> ToggleCompleteAsync(int userId, int taskId)
    {
        var task = await _uow.Tasks.GetByIdAsync(taskId)
            ?? throw new KeyNotFoundException("Task not found.");

        if (task.UserId != userId)
            throw new UnauthorizedAccessException("Not your task.");

        task.IsCompleted = !task.IsCompleted;
        _uow.Tasks.Update(task);
        await _uow.SaveChangesAsync();

        _cache.Remove(UserCacheKey(userId));
        _cache.Remove(AllTasksCacheKey);

        return MapToDto(task);
    }

    private static TaskResponseDto MapToDto(TaskItem t) =>
        new(t.Id, t.Title, t.Description, t.IsCompleted, t.CreatedAt, t.UserId);
}