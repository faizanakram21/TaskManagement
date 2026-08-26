using Microsoft.EntityFrameworkCore;
using TaskManagement.Domain.Interfaces;
using TaskManagement.Domain.Entities;
using TaskManagement.Infrastructure.Persistence;

namespace TaskManagement.Infrastructure.Repositories;

public class TaskRepository : ITaskRepository
{
    private readonly AppDbContext _context;

    public TaskRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TaskItem>> GetByUserIdAsync(int userId)
        => await _context.Tasks.Where(t => t.UserId == userId).ToListAsync();

    public async Task<IEnumerable<TaskItem>> GetAllAsync()
        => await _context.Tasks.OrderByDescending(t => t.CreatedAt).ToListAsync();

    public async Task<TaskItem?> GetByIdAsync(int id)
        => await _context.Tasks.FindAsync(id);

    public async Task AddAsync(TaskItem task)
        => await _context.Tasks.AddAsync(task);

    public void Update(TaskItem task)
        => _context.Tasks.Update(task);

    public void Delete(TaskItem task)
        => _context.Tasks.Remove(task);
}