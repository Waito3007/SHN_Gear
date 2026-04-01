using SHNGearBE.Models.Enums;

namespace SHNGearBE.Infrastructure.Payment;

public class PayPalPaymentStrategy : IPaymentStrategy
{
    private readonly IPayPalGatewayService _payPalGatewayService;

    public PayPalPaymentStrategy(IPayPalGatewayService payPalGatewayService)
    {
        _payPalGatewayService = payPalGatewayService;
    }

    public PaymentProvider Provider => PaymentProvider.PayPal;

    public async Task<PaymentResult> ProcessAsync(PaymentContext context, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(context.PaymentToken))
        {
            return PaymentResult.Failed("Missing PayPal order id");
        }

        var captureResult = await _payPalGatewayService.CaptureOrderAsync(context.PaymentToken, cancellationToken);
        if (!captureResult.Success)
        {
            return PaymentResult.Failed(captureResult.ErrorMessage ?? "PayPal capture failed");
        }

        return PaymentResult.Succeeded(captureResult.CaptureId ?? context.PaymentToken);
    }
}