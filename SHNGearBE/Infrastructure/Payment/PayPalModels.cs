namespace SHNGearBE.Infrastructure.Payment;

public class PayPalCreateOrderResult
{
    public string OrderId { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = "USD";
    public decimal AmountUsd { get; set; }
    public decimal AppliedVndPerUsdRate { get; set; }
    public bool UsedFallbackRate { get; set; }
}

public class PayPalCaptureResult
{
    public bool Success { get; set; }
    public string? CaptureId { get; set; }
    public string? ErrorMessage { get; set; }
}

public class PayPalRefundResult
{
    public bool Success { get; set; }
    public string? RefundId { get; set; }
    public decimal? RefundedAmountUsd { get; set; }
    public string? ErrorMessage { get; set; }
}

public class PayPalCaptureAmountResult
{
    public bool Success { get; set; }
    public decimal? AmountUsd { get; set; }
    public string CurrencyCode { get; set; } = "USD";
    public string? ErrorMessage { get; set; }
}

public class PayPalConversionResult
{
    public decimal AmountUsd { get; set; }
    public decimal AppliedVndPerUsdRate { get; set; }
    public bool UsedFallbackRate { get; set; }
}

public class PayPalWebhookNotification
{
    public string EventType { get; set; } = string.Empty;
    public string? CaptureId { get; set; }
}