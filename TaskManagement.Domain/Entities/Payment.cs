namespace TaskManagement.Domain.Entities;

public class Payment
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string StripePaymentIntentId { get; set; } = string.Empty;
    public long Amount { get; set; }          // cents me store hoga
    public string Currency { get; set; } = "usd";
    public string Status { get; set; } = "pending"; // pending, succeeded, failed
    public string PlanType { get; set; } = "monthly"; // monthly/yearly
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
}