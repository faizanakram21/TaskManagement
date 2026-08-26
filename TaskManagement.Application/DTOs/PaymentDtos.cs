namespace TaskManagement.Application.DTOs;

public record CreatePaymentIntentDto(string PlanType); // "monthly" ya "yearly"

public record PaymentIntentResponseDto(string ClientSecret, string PublishableKey);

public record PaymentStatusDto(string Status, bool IsPro, DateTime? ProExpiresAt);