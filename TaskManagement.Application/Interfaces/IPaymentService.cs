using TaskManagement.Application.DTOs;

namespace TaskManagement.Application.Interfaces;

public interface IPaymentService
{
    Task<PaymentIntentResponseDto> CreatePaymentIntentAsync(int userId, CreatePaymentIntentDto dto);
    Task HandleWebhookAsync(string json, string stripeSignature);
}