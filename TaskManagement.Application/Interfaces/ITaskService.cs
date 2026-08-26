using TaskManagement.Application.DTOs;

namespace TaskManagement.Application.Interfaces;

public interface ITaskService
{
    Task<IEnumerable<TaskResponseDto>> GetMyTasksAsync(int userId);
    Task<IEnumerable<TaskResponseDto>> GetAllTasksAsync();
    Task<TaskResponseDto> CreateAsync(int userId, CreateTaskDto dto);
    Task<TaskResponseDto> UpdateAsync(int userId, int taskId, UpdateTaskDto dto);
    Task DeleteAsync(int userId, int taskId);
    Task<TaskResponseDto> ToggleCompleteAsync(int userId, int taskId);
}