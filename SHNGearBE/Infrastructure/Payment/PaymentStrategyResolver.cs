using SHNGearBE.Models.Enums;

namespace SHNGearBE.Infrastructure.Payment;

public interface IPaymentStrategyResolver
{
    IPaymentStrategy Resolve(PaymentProvider provider);
}

public class PaymentStrategyResolver : IPaymentStrategyResolver
{
    private readonly IReadOnlyDictionary<PaymentProvider, IPaymentStrategy> _strategies;

    public PaymentStrategyResolver(IEnumerable<IPaymentStrategy> strategies)
    {
        _strategies = strategies.ToDictionary(s => s.Provider, s => s);
    }

    public IPaymentStrategy Resolve(PaymentProvider provider)
    {
        if (_strategies.TryGetValue(provider, out var strategy))
        {
            return strategy;
        }

        throw new InvalidOperationException($"Payment provider not configured: {provider}");
    }
}
