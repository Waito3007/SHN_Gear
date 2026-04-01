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
            $"If this was not you, ignore this email.\n" +
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
}
