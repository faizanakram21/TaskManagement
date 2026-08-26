using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Interfaces;
using TaskManagement.Infrastructure.Repositories;

namespace TaskManagement.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private UserRepository? _users;
    private TaskRepository? _tasks;
    private PaymentRepository? _payments;   // ✅ nayi line

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IUserRepository Users
        => _users ??= new UserRepository(_context);

    public ITaskRepository Tasks
        => _tasks ??= new TaskRepository(_context);

    public IPaymentRepository Payments        // ✅ nayi property
        => _payments ??= new PaymentRepository(_context);

    public async Task<int> SaveChangesAsync()
        => await _context.SaveChangesAsync();

    public void Dispose()
        => _context.Dispose();
}