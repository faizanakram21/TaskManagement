using Microsoft.EntityFrameworkCore;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<TaskItem> Tasks { get; }
    System.Threading.Tasks.Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}