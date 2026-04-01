using SHNGearMailService.Abstractions;
using SHNGearMailService.Models;

namespace SHNGearMailService.Templates;

public sealed class OtpEmailTemplateRenderer : IEmailTemplateRenderer
{
  public RenderedEmailTemplate RenderOtpTemplate(OtpEmailTemplateType type, OtpEmailTemplateData data)
  {
    var (title, subtitle, subject) = type switch
    {
      OtpEmailTemplateType.VerifyEmail => (
          "Verify your account",
          "Use this one-time password to complete email verification.",
          $"[{data.AppName}] Email verification OTP"),
      OtpEmailTemplateType.ForgotPassword => (
          "Reset your password",
          "Use this one-time password to continue resetting your password.",
          $"[{data.AppName}] Password reset OTP"),
      _ => (
          "One-time password",
          "Use this code to continue.",
          $"[{data.AppName}] OTP")
    };

    var html = $@"
<!doctype html>
<html lang=""en""><head>
<meta charset=""utf-8"" />
<meta name=""viewport"" content=""width=device-width, initial-scale=1"" />
<title>{subject}</title>
</head>
<body style=""margin:0;padding:0;background:#f5f7fb;font-family:Segoe UI,Arial,sans-serif;color:#111827;"">
  <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""padding:32px 12px;"">
    <tr>
      <td align=""center"">
        <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""max-width:620px;background:#ffffff;border-radius:18px;overflow:hidden;border:1px solid #e5e7eb;box-shadow:0 10px 30px rgba(17,24,39,.08);"">
          <tr>
            <td style=""padding:20px 24px;background:linear-gradient(120deg,#111827,#1f2937);color:#fff;"">
              <div style=""font-size:18px;font-weight:700;letter-spacing:.2px;"">{Escape(data.AppName)}</div>
              <div style=""margin-top:4px;font-size:12px;opacity:.9;"">Security verification message</div>
            </td>
          </tr>
          <tr>
            <td style=""padding:28px 24px 16px 24px;"">
              <h2 style=""margin:0 0 8px 0;font-size:24px;line-height:1.25;color:#111827;"">{title}</h2>
              <p style=""margin:0;color:#4b5563;font-size:14px;line-height:1.6;"">Hello {Escape(data.RecipientName)}, {subtitle}</p>
            </td>
          </tr>
          <tr>
            <td style=""padding:6px 24px 8px 24px;"">
              <div style=""padding:18px;border:1px dashed #d1d5db;border-radius:14px;background:#f9fafb;text-align:center;"">
                <div style=""font-size:12px;color:#6b7280;letter-spacing:.3px;text-transform:uppercase;"">Your OTP code</div>
                <div style=""margin-top:8px;font-size:36px;line-height:1;letter-spacing:8px;font-weight:800;color:#111827;"">{Escape(data.OtpCode)}</div>
              </div>
            </td>
          </tr>
          <tr>
            <td style=""padding:8px 24px 0 24px;"">
              <p style=""margin:0;color:#374151;font-size:13px;line-height:1.6;"">This code expires in <b>{data.ExpiryMinutes} minutes</b>. If you did not request this code, ignore this email.</p>
            </td>
          </tr>
          <tr>
            <td style=""padding:18px 24px 24px 24px;"">
              <p style=""margin:0;color:#6b7280;font-size:12px;line-height:1.6;"">Need help? Contact us at <a href=""mailto:{Escape(data.SupportEmail)}"" style=""color:#111827;text-decoration:none;font-weight:600;"">{Escape(data.SupportEmail)}</a>.</p>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body></html>";

    var text =
        $"{data.AppName}\n" +
        $"{title}\n\n" +
        $"Hello {data.RecipientName},\n{subtitle}\n\n" +
        $"OTP: {data.OtpCode}\n" +
        $"Expires in {data.ExpiryMinutes} minutes.\n\n" +
        "If this was not you, ignore this email.\n" +
        $"Support: {data.SupportEmail}";

    return new RenderedEmailTemplate
    {
      Subject = subject,
      HtmlBody = html,
      TextBody = text
    };
  }

  public RenderedEmailTemplate RenderOrderPlacedTemplate(OrderPlacedEmailTemplateData data)
  {
    var subject = $"[{data.AppName}] Order {data.OrderCode} confirmed";
    var createdAt = data.CreatedAtUtc.ToString("yyyy-MM-dd HH:mm 'UTC'");
    var rows = string.Join(
        string.Empty,
        data.Items.Select(item =>
            $@"<tr>
  <td style=""padding:10px 12px;border-top:1px solid #e5e7eb;color:#111827;font-size:13px;"">{Escape(item.ProductName)}<div style=""color:#6b7280;font-size:12px;margin-top:2px;"">{Escape(item.VariantName)} | SKU: {Escape(item.Sku)}</div></td>
  <td style=""padding:10px 12px;border-top:1px solid #e5e7eb;color:#111827;font-size:13px;text-align:center;"">{item.Quantity}</td>
  <td style=""padding:10px 12px;border-top:1px solid #e5e7eb;color:#111827;font-size:13px;text-align:right;"">{FormatCurrency(item.SubTotal, data.CurrencyCode)}</td>
</tr>"));

    var html = $@"
<!doctype html>
<html lang=""en""><head>
<meta charset=""utf-8"" />
<meta name=""viewport"" content=""width=device-width, initial-scale=1"" />
<title>{Escape(subject)}</title>
</head>
<body style=""margin:0;padding:0;background:#f3f5f9;font-family:Segoe UI,Arial,sans-serif;color:#111827;"">
<table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""padding:28px 12px;"">
<tr><td align=""center"">
<table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""max-width:680px;background:#ffffff;border:1px solid #e5e7eb;border-radius:16px;overflow:hidden;box-shadow:0 16px 42px rgba(15,23,42,.08);"">
<tr>
<td style=""padding:22px 24px;background:linear-gradient(120deg,#0f172a,#111827);color:#fff;"">
<div style=""font-size:19px;font-weight:700;letter-spacing:.2px;"">{Escape(data.AppName)}</div>
<div style=""margin-top:6px;font-size:12px;opacity:.9;"">Order confirmation</div>
</td>
</tr>
<tr>
<td style=""padding:24px 24px 10px 24px;"">
<h2 style=""margin:0;font-size:24px;line-height:1.3;color:#0f172a;"">Your order is placed successfully</h2>
<p style=""margin:10px 0 0 0;color:#475569;font-size:14px;line-height:1.7;"">Hi {Escape(data.RecipientName)}, thanks for shopping with us. We have received your order and started processing it.</p>
</td>
</tr>
<tr>
<td style=""padding:12px 24px 4px 24px;"">
<table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""border:1px solid #e5e7eb;border-radius:12px;background:#f8fafc;"">
<tr>
<td style=""padding:12px 14px;font-size:13px;color:#475569;"">Order code</td>
<td style=""padding:12px 14px;font-size:13px;color:#0f172a;font-weight:700;text-align:right;"">{Escape(data.OrderCode)}</td>
</tr>
<tr>
<td style=""padding:12px 14px;font-size:13px;color:#475569;border-top:1px solid #e5e7eb;"">Created at</td>
<td style=""padding:12px 14px;font-size:13px;color:#0f172a;text-align:right;border-top:1px solid #e5e7eb;"">{Escape(createdAt)}</td>
</tr>
<tr>
<td style=""padding:12px 14px;font-size:13px;color:#475569;border-top:1px solid #e5e7eb;"">Payment method</td>
<td style=""padding:12px 14px;font-size:13px;color:#0f172a;text-align:right;border-top:1px solid #e5e7eb;"">{Escape(data.PaymentMethod)}</td>
</tr>
<tr>
<td style=""padding:12px 14px;font-size:13px;color:#475569;border-top:1px solid #e5e7eb;"">Payment status</td>
<td style=""padding:12px 14px;font-size:13px;color:#0f172a;text-align:right;border-top:1px solid #e5e7eb;"">{Escape(data.PaymentStatus)}</td>
</tr>
</table>
</td>
</tr>
<tr>
<td style=""padding:14px 24px 0 24px;"">
<table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""border:1px solid #e5e7eb;border-radius:12px;overflow:hidden;"">
<thead>
<tr style=""background:#f8fafc;"">
<th style=""text-align:left;padding:10px 12px;color:#475569;font-size:12px;text-transform:uppercase;letter-spacing:.3px;"">Item</th>
<th style=""text-align:center;padding:10px 12px;color:#475569;font-size:12px;text-transform:uppercase;letter-spacing:.3px;"">Qty</th>
<th style=""text-align:right;padding:10px 12px;color:#475569;font-size:12px;text-transform:uppercase;letter-spacing:.3px;"">Subtotal</th>
</tr>
</thead>
<tbody>{rows}</tbody>
</table>
</td>
</tr>
<tr>
<td style=""padding:14px 24px 10px 24px;"">
<table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""max-width:340px;margin-left:auto;"">
<tr><td style=""padding:4px 0;color:#475569;font-size:13px;"">Subtotal</td><td style=""padding:4px 0;color:#111827;font-size:13px;text-align:right;"">{FormatCurrency(data.SubTotal, data.CurrencyCode)}</td></tr>
<tr><td style=""padding:4px 0;color:#475569;font-size:13px;"">Shipping</td><td style=""padding:4px 0;color:#111827;font-size:13px;text-align:right;"">{FormatCurrency(data.ShippingFee, data.CurrencyCode)}</td></tr>
<tr><td style=""padding:8px 0 0 0;border-top:1px solid #e5e7eb;color:#0f172a;font-size:15px;font-weight:700;"">Total</td><td style=""padding:8px 0 0 0;border-top:1px solid #e5e7eb;color:#0f172a;font-size:15px;font-weight:700;text-align:right;"">{FormatCurrency(data.TotalAmount, data.CurrencyCode)}</td></tr>
</table>
</td>
</tr>
<tr>
<td style=""padding:6px 24px 22px 24px;"">
<p style=""margin:0;color:#64748b;font-size:12px;line-height:1.7;"">Delivery address: {Escape(data.DeliveryAddress ?? "N/A")}</p>
<p style=""margin:4px 0 0 0;color:#64748b;font-size:12px;line-height:1.7;"">Need help? Contact <a href=""mailto:{Escape(data.SupportEmail)}"" style=""color:#0f172a;text-decoration:none;font-weight:600;"">{Escape(data.SupportEmail)}</a>.</p>
</td>
</tr>
</table>
</td></tr>
</table>
</body></html>";

    var textItems = string.Join(
        "\n",
        data.Items.Select(i => $"- {i.ProductName} ({i.VariantName}) x{i.Quantity}: {FormatCurrency(i.SubTotal, data.CurrencyCode)}"));

    var text =
        $"{data.AppName}\n" +
        "Order confirmation\n\n" +
        $"Hello {data.RecipientName}, your order has been placed successfully.\n" +
        $"Order code: {data.OrderCode}\n" +
        $"Created at: {createdAt}\n" +
        $"Payment method: {data.PaymentMethod}\n" +
        $"Payment status: {data.PaymentStatus}\n\n" +
        $"Items:\n{textItems}\n\n" +
        $"Subtotal: {FormatCurrency(data.SubTotal, data.CurrencyCode)}\n" +
        $"Shipping: {FormatCurrency(data.ShippingFee, data.CurrencyCode)}\n" +
        $"Total: {FormatCurrency(data.TotalAmount, data.CurrencyCode)}\n\n" +
        $"Delivery address: {data.DeliveryAddress}\n" +
        $"Support: {data.SupportEmail}";

    return new RenderedEmailTemplate
    {
      Subject = subject,
      HtmlBody = html,
      TextBody = text
    };
  }

  public RenderedEmailTemplate RenderRefundCompletedTemplate(RefundCompletedEmailTemplateData data)
  {
    var subject = $"[{data.AppName}] Refund completed for order {data.OrderCode}";
    var refundedAt = data.RefundedAtUtc.ToString("yyyy-MM-dd HH:mm 'UTC'");
    var refundedAmount = FormatCurrency(data.RefundedAmount, data.CurrencyCode);
    var totalCaptured = FormatCurrency(data.TotalCapturedAmount, data.CurrencyCode);

    var html = $@"
    <!doctype html>
    <html lang=""en""><head>
    <meta charset=""utf-8"" />
    <meta name=""viewport"" content=""width=device-width, initial-scale=1"" />
    <title>{Escape(subject)}</title>
    </head>
    <body style=""margin:0;padding:0;background:#f3f5f9;font-family:Segoe UI,Arial,sans-serif;color:#111827;"">
    <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""padding:28px 12px;"">
    <tr><td align=""center"">
    <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""max-width:660px;background:#ffffff;border:1px solid #e5e7eb;border-radius:16px;overflow:hidden;box-shadow:0 16px 42px rgba(15,23,42,.08);"">
    <tr>
    <td style=""padding:22px 24px;background:linear-gradient(120deg,#0f172a,#111827);color:#fff;"">
    <div style=""font-size:19px;font-weight:700;letter-spacing:.2px;"">{Escape(data.AppName)}</div>
    <div style=""margin-top:6px;font-size:12px;opacity:.9;"">Refund confirmation</div>
    </td>
    </tr>
    <tr>
    <td style=""padding:24px 24px 10px 24px;"">
    <h2 style=""margin:0;font-size:24px;line-height:1.3;color:#0f172a;"">Your refund has been completed</h2>
    <p style=""margin:10px 0 0 0;color:#475569;font-size:14px;line-height:1.7;"">Hi {Escape(data.RecipientName)}, we have processed your refund successfully.</p>
    </td>
    </tr>
    <tr>
    <td style=""padding:12px 24px 4px 24px;"">
    <table role=""presentation"" width=""100%"" cellspacing=""0"" cellpadding=""0"" style=""border:1px solid #e5e7eb;border-radius:12px;background:#f8fafc;"">
    <tr>
    <td style=""padding:12px 14px;font-size:13px;color:#475569;"">Order code</td>
    <td style=""padding:12px 14px;font-size:13px;color:#0f172a;font-weight:700;text-align:right;"">{Escape(data.OrderCode)}</td>
    </tr>
    <tr>
    <td style=""padding:12px 14px;font-size:13px;color:#475569;border-top:1px solid #e5e7eb;"">Refunded at</td>
    <td style=""padding:12px 14px;font-size:13px;color:#0f172a;text-align:right;border-top:1px solid #e5e7eb;"">{Escape(refundedAt)}</td>
    </tr>
    <tr>
    <td style=""padding:12px 14px;font-size:13px;color:#475569;border-top:1px solid #e5e7eb;"">Refund amount</td>
    <td style=""padding:12px 14px;font-size:13px;color:#0f172a;text-align:right;border-top:1px solid #e5e7eb;"">{Escape(refundedAmount)}</td>
    </tr>
    <tr>
    <td style=""padding:12px 14px;font-size:13px;color:#475569;border-top:1px solid #e5e7eb;"">Refund progress</td>
    <td style=""padding:12px 14px;font-size:13px;color:#0f172a;text-align:right;border-top:1px solid #e5e7eb;"">{Escape(refundedAmount)} / {Escape(totalCaptured)}</td>
    </tr>
    </table>
    </td>
    </tr>
    <tr>
    <td style=""padding:8px 24px 22px 24px;"">
    <p style=""margin:0;color:#64748b;font-size:12px;line-height:1.7;"">Refund reason: {Escape(string.IsNullOrWhiteSpace(data.RefundReason) ? "N/A" : data.RefundReason)}</p>
    <p style=""margin:4px 0 0 0;color:#64748b;font-size:12px;line-height:1.7;"">Need help? Contact <a href=""mailto:{Escape(data.SupportEmail)}"" style=""color:#0f172a;text-decoration:none;font-weight:600;"">{Escape(data.SupportEmail)}</a>.</p>
    </td>
    </tr>
    </table>
    </td></tr>
    </table>
    </body></html>";

    var text =
      $"{data.AppName}\n" +
      "Refund confirmation\n\n" +
      $"Hello {data.RecipientName}, your refund has been completed.\n" +
      $"Order code: {data.OrderCode}\n" +
      $"Refunded at: {refundedAt}\n" +
      $"Refund amount: {refundedAmount}\n" +
      $"Refund progress: {refundedAmount} / {totalCaptured}\n" +
      $"Refund reason: {(string.IsNullOrWhiteSpace(data.RefundReason) ? "N/A" : data.RefundReason)}\n" +
      $"Support: {data.SupportEmail}";

    return new RenderedEmailTemplate
    {
      Subject = subject,
      HtmlBody = html,
      TextBody = text
    };
  }

  private static string Escape(string value)
  {
    return System.Net.WebUtility.HtmlEncode(value ?? string.Empty);
  }

  private static string FormatCurrency(decimal amount, string currencyCode)
  {
    return currencyCode.ToUpperInvariant() == "USD"
        ? string.Format(System.Globalization.CultureInfo.GetCultureInfo("en-US"), "{0:C2}", amount)
        : string.Format(System.Globalization.CultureInfo.GetCultureInfo("vi-VN"), "{0:C0}", amount);
  }
}
