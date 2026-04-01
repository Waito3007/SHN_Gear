using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace SHNGearBE.Infrastructure.Payment;

public interface IPayPalGatewayService
{
    Task<PayPalCreateOrderResult> CreateOrderAsync(decimal amountVnd, string referenceId, CancellationToken cancellationToken = default);
    Task<PayPalCaptureResult> CaptureOrderAsync(string paypalOrderId, CancellationToken cancellationToken = default);
    Task<PayPalCaptureAmountResult> GetCaptureAmountAsync(string captureId, CancellationToken cancellationToken = default);
    Task<PayPalRefundResult> RefundCaptureAsync(string captureId, decimal? amountUsd = null, CancellationToken cancellationToken = default);
    Task<bool> VerifyWebhookSignatureAsync(IHeaderDictionary headers, JsonElement webhookEvent, CancellationToken cancellationToken = default);
    Task<PayPalWebhookNotification> ParseWebhookAsync(JsonElement webhookEvent, CancellationToken cancellationToken = default);
    string GetClientId();
    string GetCurrencyCode();
}