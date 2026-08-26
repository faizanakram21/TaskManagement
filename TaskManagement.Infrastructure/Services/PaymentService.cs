using Microsoft.Extensions.Configuration;
using Stripe;
using TaskManagement.Application.DTOs;
using TaskManagement.Application.Interfaces;
using TaskManagement.Domain.Entities;

namespace TaskManagement.Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly IUnitOfWork _uow;
    private readonly IConfiguration _config;

    public PaymentService(IUnitOfWork uow, IConfiguration config)
    {
        _uow = uow;
        _config = config;
        StripeConfiguration.ApiKey = _config["Stripe:SecretKey"];
    }

    public async Task<PaymentIntentResponseDto> CreatePaymentIntentAsync(int userId, CreatePaymentIntentDto dto)
    {
        long amount = dto.PlanType == "yearly" ? 9900 : 999; // cents ($99/yr, $9.99/mo)

        var options = new PaymentIntentCreateOptions
        {
            Amount = amount,
            Currency = "usd",
            Metadata = new Dictionary<string, string>
            {
                { "userId", userId.ToString() },
                { "planType", dto.PlanType }
            }
        };

        var service = new PaymentIntentService();
        PaymentIntent intent = await service.CreateAsync(options);

        var payment = new Payment
        {
            UserId = userId,
            StripePaymentIntentId = intent.Id,
            Amount = amount,
            PlanType = dto.PlanType,
            Status = "pending"
        };

        await _uow.Payments.AddAsync(payment);
        await _uow.SaveChangesAsync();

        return new PaymentIntentResponseDto(intent.ClientSecret, _config["Stripe:PublishableKey"]!);
    }

    public async Task HandleWebhookAsync(string json, string stripeSignature)
    {
        var webhookSecret = _config["Stripe:WebhookSecret"];
        var stripeEvent = EventUtility.ConstructEvent(json, stripeSignature, webhookSecret);

        if (stripeEvent.Type == "payment_intent.succeeded")
        {
            var intent = (PaymentIntent)stripeEvent.Data.Object;
            var payment = await _uow.Payments.GetByIntentIdAsync(intent.Id);

            if (payment != null && payment.User != null)
            {
                payment.Status = "succeeded";
                payment.User.IsPro = true;
                payment.User.ProExpiresAt = payment.PlanType == "yearly"
                    ? DateTime.UtcNow.AddYears(1)
                    : DateTime.UtcNow.AddMonths(1);

                await _uow.SaveChangesAsync();  // dono changes ek sath save honge
            }
        }
    }
}