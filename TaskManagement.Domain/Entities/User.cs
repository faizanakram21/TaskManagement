namespace TaskManagement.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // ✅ Payment/Subscription fields
    public bool IsPro { get; set; } = false;
    public DateTime? ProExpiresAt { get; set; }

    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();

    // ✅ Nayi line add karo
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}