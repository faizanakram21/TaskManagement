using TaskManagement.Domain.Interfaces;

namespace TaskManagement.Application.Interfaces;

public interface IUnitOfWork : IDisposable
{
    IUserRepository Users { get; }
    ITaskRepository Tasks { get; }
    IPaymentRepository Payments { get; }   // ✅ nayi line

    Task<int> SaveChangesAsync();
}