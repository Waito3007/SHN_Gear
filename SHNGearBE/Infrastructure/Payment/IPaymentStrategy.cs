using SHNGearBE.Models.Enums;

namespace SHNGearBE.Infrastructure.Payment;

public interface IPaymentStrategy
{
    PaymentProvider Provider { get; }
    Task<PaymentResult> ProcessAsync(PaymentContext context, CancellationToken cancellationToken = default);
}
