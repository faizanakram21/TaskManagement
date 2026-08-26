using TaskManagement.Domain.Entities;

namespace TaskManagement.Domain.Interfaces;  // 👈 changed

public interface ITaskRepository
{
    Task<IEnumerable<TaskItem>> GetByUserIdAsync(int userId);
    Task<IEnumerable<TaskItem>> GetAllAsync();
    Task<TaskItem?> GetByIdAsync(int id);
    Task AddAsync(TaskItem task);
    void Update(TaskItem task);
    void Delete(TaskItem task);
}