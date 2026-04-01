using System.Globalization;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using SHNGearBE.Configurations;

namespace SHNGearBE.Infrastructure.Payment;

public class PayPalGatewayService : IPayPalGatewayService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly HttpClient _httpClient;
    private readonly PayPalSettings _settings;

    public PayPalGatewayService(HttpClient httpClient, IOptions<PayPalSettings> settings)
    {
        _httpClient = httpClient;
        _settings = settings.Value;
    }

    public string GetClientId() => _settings.ClientId;

    public string GetCurrencyCode() => _settings.CurrencyCode;

    public async Task<PayPalCreateOrderResult> CreateOrderAsync(decimal amountVnd, string referenceId, CancellationToken cancellationToken = default)
    {
        var conversion = await ConvertVndToUsdAsync(amountVnd, cancellationToken);
        var accessToken = await GetAccessTokenAsync(cancellationToken);

        var payload = new
        {
            intent = "CAPTURE",
            purchase_units = new[]
            {
                new
                {
                    reference_id = referenceId,
                    amount = new
                    {
                        currency_code = _settings.CurrencyCode,
                        value = conversion.AmountUsd.ToString("0.00", CultureInfo.InvariantCulture)
                    }
                }
            }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "/v2/checkout/orders");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"PayPal create order failed: {(int)response.StatusCode} - {body}");
        }

        using var doc = JsonDocument.Parse(body);
        var orderId = doc.RootElement.GetProperty("id").GetString();
        if (string.IsNullOrWhiteSpace(orderId))
        {
            throw new InvalidOperationException("PayPal order id is missing in create order response");
        }

        return new PayPalCreateOrderResult
        {
            OrderId = orderId,
            CurrencyCode = _settings.CurrencyCode,
            AmountUsd = conversion.AmountUsd,
            AppliedVndPerUsdRate = conversion.AppliedVndPerUsdRate,
            UsedFallbackRate = conversion.UsedFallbackRate
        };
    }

    public async Task<PayPalCaptureResult> CaptureOrderAsync(string paypalOrderId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(paypalOrderId))
        {
            return new PayPalCaptureResult { Success = false, ErrorMessage = "PayPal order id is required" };
        }

        var accessToken = await GetAccessTokenAsync(cancellationToken);

        using var request = new HttpRequestMessage(HttpMethod.Post, $"/v2/checkout/orders/{paypalOrderId}/capture");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = new StringContent("{}", Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return new PayPalCaptureResult
            {
                Success = false,
                ErrorMessage = $"PayPal capture failed: {(int)response.StatusCode}"
            };
        }

        using var doc = JsonDocument.Parse(body);
        var status = doc.RootElement.TryGetProperty("status", out var statusEl) ? statusEl.GetString() : null;
        if (!string.Equals(status, "COMPLETED", StringComparison.OrdinalIgnoreCase))
        {
            return new PayPalCaptureResult
            {
                Success = false,
                ErrorMessage = "PayPal payment not completed"
            };
        }

        var captureId = TryGetCaptureId(doc.RootElement);
        return new PayPalCaptureResult
        {
            Success = true,
            CaptureId = captureId
        };
    }

    public async Task<PayPalCaptureAmountResult> GetCaptureAmountAsync(string captureId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(captureId))
        {
            return new PayPalCaptureAmountResult { Success = false, ErrorMessage = "PayPal capture id is required" };
        }

        var accessToken = await GetAccessTokenAsync(cancellationToken);
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/v2/payments/captures/{captureId}");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        var response = await _httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return new PayPalCaptureAmountResult
            {
                Success = false,
                ErrorMessage = $"PayPal capture lookup failed: {(int)response.StatusCode}"
            };
        }

        using var doc = JsonDocument.Parse(body);
        if (!doc.RootElement.TryGetProperty("amount", out var amountEl)
            || !amountEl.TryGetProperty("value", out var valueEl)
            || !decimal.TryParse(valueEl.GetString(), NumberStyles.Number, CultureInfo.InvariantCulture, out var amountUsd))
        {
            return new PayPalCaptureAmountResult
            {
                Success = false,
                ErrorMessage = "Khong doc duoc tong tien capture tu PayPal"
            };
        }

        var currency = amountEl.TryGetProperty("currency_code", out var currencyEl)
            ? currencyEl.GetString() ?? _settings.CurrencyCode
            : _settings.CurrencyCode;

        return new PayPalCaptureAmountResult
        {
            Success = true,
            AmountUsd = amountUsd,
            CurrencyCode = currency
        };
    }

    public async Task<PayPalRefundResult> RefundCaptureAsync(string captureId, decimal? amountUsd = null, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(captureId))
        {
            return new PayPalRefundResult { Success = false, ErrorMessage = "PayPal capture id is required" };
        }

        var accessToken = await GetAccessTokenAsync(cancellationToken);

        object payload;
        if (amountUsd.HasValue)
        {
            payload = new
            {
                amount = new
                {
                    currency_code = _settings.CurrencyCode,
                    value = amountUsd.Value.ToString("0.00", CultureInfo.InvariantCulture)
                }
            };
        }
        else
        {
            payload = new { };
        }

        using var request = new HttpRequestMessage(HttpMethod.Post, $"/v2/payments/captures/{captureId}/refund");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return new PayPalRefundResult
            {
                Success = false,
                ErrorMessage = $"PayPal refund failed: {(int)response.StatusCode}"
            };
        }

        using var doc = JsonDocument.Parse(body);
        var status = doc.RootElement.TryGetProperty("status", out var statusEl) ? statusEl.GetString() : null;
        if (!string.Equals(status, "COMPLETED", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(status, "PENDING", StringComparison.OrdinalIgnoreCase))
        {
            return new PayPalRefundResult
            {
                Success = false,
                ErrorMessage = "PayPal refund is not in a completed or pending state"
            };
        }

        var refundId = doc.RootElement.TryGetProperty("id", out var refundIdEl) ? refundIdEl.GetString() : null;
        decimal? refundedAmountUsd = null;
        if (doc.RootElement.TryGetProperty("amount", out var amountEl)
            && amountEl.TryGetProperty("value", out var valueEl)
            && decimal.TryParse(valueEl.GetString(), NumberStyles.Number, CultureInfo.InvariantCulture, out var parsedRefundedAmount))
        {
            refundedAmountUsd = parsedRefundedAmount;
        }

        return new PayPalRefundResult
        {
            Success = true,
            RefundId = refundId,
            RefundedAmountUsd = refundedAmountUsd
        };
    }

    public async Task<bool> VerifyWebhookSignatureAsync(IHeaderDictionary headers, JsonElement webhookEvent, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_settings.WebhookId))
        {
            return false;
        }

        var authAlgo = headers["PAYPAL-AUTH-ALGO"].ToString();
        var certUrl = headers["PAYPAL-CERT-URL"].ToString();
        var transmissionId = headers["PAYPAL-TRANSMISSION-ID"].ToString();
        var transmissionSig = headers["PAYPAL-TRANSMISSION-SIG"].ToString();
        var transmissionTime = headers["PAYPAL-TRANSMISSION-TIME"].ToString();

        if (string.IsNullOrWhiteSpace(authAlgo)
            || string.IsNullOrWhiteSpace(certUrl)
            || string.IsNullOrWhiteSpace(transmissionId)
            || string.IsNullOrWhiteSpace(transmissionSig)
            || string.IsNullOrWhiteSpace(transmissionTime))
        {
            return false;
        }

        var accessToken = await GetAccessTokenAsync(cancellationToken);
        var payload = new
        {
            auth_algo = authAlgo,
            cert_url = certUrl,
            transmission_id = transmissionId,
            transmission_sig = transmissionSig,
            transmission_time = transmissionTime,
            webhook_id = _settings.WebhookId,
            webhook_event = webhookEvent
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, "/v1/notifications/verify-webhook-signature");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return false;
        }

        using var doc = JsonDocument.Parse(body);
        var verificationStatus = doc.RootElement.TryGetProperty("verification_status", out var statusEl)
            ? statusEl.GetString()
            : string.Empty;

        return string.Equals(verificationStatus, "SUCCESS", StringComparison.OrdinalIgnoreCase);
    }

    public Task<PayPalWebhookNotification> ParseWebhookAsync(JsonElement webhookEvent, CancellationToken cancellationToken = default)
    {
        var eventType = webhookEvent.TryGetProperty("event_type", out var eventTypeEl)
            ? eventTypeEl.GetString()
            : string.Empty;

        string? captureId = null;
        if (webhookEvent.TryGetProperty("resource", out var resource))
        {
            if (resource.TryGetProperty("id", out var resourceId))
            {
                captureId = resourceId.GetString();
            }

            if (string.Equals(eventType, "PAYMENT.CAPTURE.REFUNDED", StringComparison.OrdinalIgnoreCase)
                && resource.TryGetProperty("supplementary_data", out var supplementaryData)
                && supplementaryData.TryGetProperty("related_ids", out var relatedIds)
                && relatedIds.TryGetProperty("capture_id", out var captureIdEl))
            {
                captureId = captureIdEl.GetString();
            }
        }

        return Task.FromResult(new PayPalWebhookNotification
        {
            EventType = eventType ?? string.Empty,
            CaptureId = captureId
        });
    }

    private async Task<string> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(_settings.ClientId) || string.IsNullOrWhiteSpace(_settings.ClientSecret))
        {
            throw new InvalidOperationException("PayPal credentials are not configured");
        }

        var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{_settings.ClientId}:{_settings.ClientSecret}"));
        using var request = new HttpRequestMessage(HttpMethod.Post, "/v1/oauth2/token");
        request.Headers.Authorization = new AuthenticationHeaderValue("Basic", credentials);
        request.Content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("grant_type", "client_credentials")
        });

        var response = await _httpClient.SendAsync(request, cancellationToken);
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"PayPal token request failed: {(int)response.StatusCode} - {body}");
        }

        var token = JsonSerializer.Deserialize<PayPalTokenResponse>(body, JsonOptions)?.AccessToken;
        if (string.IsNullOrWhiteSpace(token))
        {
            throw new InvalidOperationException("PayPal access token is missing");
        }

        return token;
    }

    private async Task<PayPalConversionResult> ConvertVndToUsdAsync(decimal amountVnd, CancellationToken cancellationToken)
    {
        if (amountVnd <= 0)
        {
            throw new InvalidOperationException("Invalid VND amount for PayPal order");
        }

        var fallbackRate = _settings.FallbackVndPerUsdRate <= 0 ? 25500m : _settings.FallbackVndPerUsdRate;

        try
        {
            if (string.IsNullOrWhiteSpace(_settings.ExchangeRateApiKey))
            {
                throw new InvalidOperationException("Exchange rate API key is not configured");
            }

            var rateApiBaseUrl = string.IsNullOrWhiteSpace(_settings.ExchangeRateApiBaseUrl)
                ? "https://v6.exchangerate-api.com"
                : _settings.ExchangeRateApiBaseUrl.TrimEnd('/');
            var path = $"{rateApiBaseUrl}/v6/{_settings.ExchangeRateApiKey}/latest/VND";
            var response = await _httpClient.GetAsync(path, cancellationToken);
            var body = await response.Content.ReadAsStringAsync(cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidOperationException("Exchange rate provider returned non-success status");
            }

            using var doc = JsonDocument.Parse(body);
            var usdRate = (decimal)doc.RootElement
                .GetProperty("conversion_rates")
                .GetProperty("USD")
                .GetDouble();

            var amountUsd = Math.Round(amountVnd * usdRate, 2, MidpointRounding.AwayFromZero);
            if (amountUsd < 0.01m)
            {
                amountUsd = 0.01m;
            }

            var effectiveVndPerUsd = usdRate > 0 ? Math.Round(1m / usdRate, 2, MidpointRounding.AwayFromZero) : fallbackRate;
            return new PayPalConversionResult
            {
                AmountUsd = amountUsd,
                AppliedVndPerUsdRate = effectiveVndPerUsd,
                UsedFallbackRate = false
            };
        }
        catch
        {
            var amountUsd = Math.Round(amountVnd / fallbackRate, 2, MidpointRounding.AwayFromZero);
            if (amountUsd < 0.01m)
            {
                amountUsd = 0.01m;
            }

            return new PayPalConversionResult
            {
                AmountUsd = amountUsd,
                AppliedVndPerUsdRate = fallbackRate,
                UsedFallbackRate = true
            };
        }
    }

    private static string? TryGetCaptureId(JsonElement root)
    {
        if (!root.TryGetProperty("purchase_units", out var purchaseUnits)
            || purchaseUnits.ValueKind != JsonValueKind.Array
            || purchaseUnits.GetArrayLength() == 0)
        {
            return null;
        }

        var firstUnit = purchaseUnits[0];
        if (!firstUnit.TryGetProperty("payments", out var payments)
            || !payments.TryGetProperty("captures", out var captures)
            || captures.ValueKind != JsonValueKind.Array
            || captures.GetArrayLength() == 0)
        {
            return null;
        }

        return captures[0].TryGetProperty("id", out var captureIdEl) ? captureIdEl.GetString() : null;
    }

    private sealed class PayPalTokenResponse
    {
        [JsonPropertyName("access_token")]
        public string AccessToken { get; set; } = string.Empty;
    }
}