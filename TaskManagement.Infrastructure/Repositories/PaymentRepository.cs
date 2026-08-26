using Microsoft.EntityFrameworkCore;
using TaskManagement.Domain.Entities;
using TaskManagement.Domain.Interfaces;
using TaskManagement.Infrastructure.Persistence;

namespace TaskManagement.Infrastructure.Repositories;

public class PaymentRepository : IPaymentRepository
{
    private readonly AppDbContext _context;

    public PaymentRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task AddAsync(Payment payment)
        => await _context.Payments.AddAsync(payment);

    public async Task<Payment?> GetByIntentIdAsync(string intentId)
        => await _context.Payments
            .Include(p => p.User)   // User bhi load hoga, webhook me IsPro update karne ke liye
            .FirstOrDefaultAsync(p => p.StripePaymentIntentId == intentId);
}