using SHNGearBE.Models.Enums;

namespace SHNGearBE.Infrastructure.Payment;

public class CodPaymentStrategy : IPaymentStrategy
{
    public PaymentProvider Provider => PaymentProvider.Cod;

    public Task<PaymentResult> ProcessAsync(PaymentContext context, CancellationToken cancellationToken = default)
    {
        var txId = $"COD-{context.OrderId:N}";
        return Task.FromResult(PaymentResult.Succeeded(txId));
    }
}
