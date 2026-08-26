using TaskManagement.Domain.Entities;

namespace TaskManagement.Domain.Interfaces;

public interface IPaymentRepository
{
    Task AddAsync(Payment payment);
    Task<Payment?> GetByIntentIdAsync(string intentId);
}