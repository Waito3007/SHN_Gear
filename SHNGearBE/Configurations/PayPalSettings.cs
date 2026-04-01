namespace SHNGearBE.Configurations;

public class PayPalSettings
{
    public const string SectionName = "PayPal";

    public string BaseUrl { get; set; } = "https://api-m.sandbox.paypal.com";
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string WebhookId { get; set; } = string.Empty;
    public string CurrencyCode { get; set; } = "USD";

    public string ExchangeRateApiBaseUrl { get; set; } = "https://v6.exchangerate-api.com";
    public string ExchangeRateApiKey { get; set; } = string.Empty;
    public decimal FallbackVndPerUsdRate { get; set; } = 25500m;
    public int HttpTimeoutSeconds { get; set; } = 20;
}