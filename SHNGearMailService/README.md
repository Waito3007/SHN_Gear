# SHNGearMailService

Thu vien gui mail tach rieng cho SHNGear, de quan ly va mo rong de dang.

## 1. Muc tieu

- Tach logic gui mail khoi business services.
- De mo rong provider trong tuong lai (SMTP, SendGrid, Mailgun, ...).
- Su dung typed settings + fallback environment variables.

## 2. Kha nang hien tai (v1)

- Gui mail qua SMTP thong qua `IEmailService`.
- Ho tro `To`, `Cc`, `Bcc`, `From` (override tuy chon).
- Ho tro `HTML` hoac `text` body.
- Tra ket qua theo `EmailSendResult` (khong throw ra service layer).

## 3. Cau truc chinh

- `Abstractions/IEmailService.cs`: hop dong gui mail.
- `Models/EmailMessage.cs`: model noi dung mail.
- `Models/EmailAddress.cs`: model dia chi nguoi gui/nhan.
- `Models/EmailServiceSettings.cs`: settings SMTP.
- `Infrastructure/SmtpEmailService.cs`: implementation SMTP.
- `Extensions/ServiceCollectionExtensions.cs`: dang ky DI + bind config.

## 4. Dang ky DI

Trong `Program.cs` cua backend:

```csharp
using SHNGearMailService.Extensions;

builder.Services.AddMailService(builder.Configuration);
```

## 5. Cau hinh

### 5.1 appsettings.json

```json
{
  "EmailService": {
    "Provider": "Smtp",
    "SmtpHost": "smtp.gmail.com",
    "SmtpPort": 587,
    "UseSsl": true,
    "Username": "noreply@your-domain.com",
    "Password": "app-password",
    "FromAddress": "noreply@your-domain.com",
    "FromDisplayName": "SHNGear",
    "TimeoutSeconds": 30
  }
}
```

### 5.2 Environment variables (uu tien fallback)

- `EMAIL_SMTP_HOST`
- `EMAIL_SMTP_PORT`
- `EMAIL_SMTP_USE_SSL`
- `EMAIL_SMTP_USERNAME`
- `EMAIL_SMTP_PASSWORD`
- `EMAIL_FROM_ADDRESS`
- `EMAIL_FROM_DISPLAY_NAME`
- `EMAIL_TIMEOUT_SECONDS`

Alias theo naming hien dang dung:

- `SMTP_HOST`
- `SMTP_PORT`
- `SENDER_EMAIL`
- `SENDER_PASSWORD`

Goi y: production nen dung env vars/secret manager, khong hard-code password trong file config.

## 6. Cach su dung

### 6.1 Inject service

```csharp
using SHNGearMailService.Abstractions;

public class OrderService
{
    private readonly IEmailService _emailService;

    public OrderService(IEmailService emailService)
    {
        _emailService = emailService;
    }
}
```

### 6.2 Vi du gui mail HTML co ban

```csharp
using SHNGearMailService.Models;

var message = new EmailMessage
{
    Subject = "[SHNGear] Order Confirmation",
    Body = "<h3>Cam on ban da dat hang</h3><p>Don hang cua ban da duoc tiep nhan.</p>",
    IsHtml = true,
    To =
    {
        new EmailAddress
        {
            Address = "customer@example.com",
            DisplayName = "Customer"
        }
    }
};

var result = await _emailService.SendAsync(message, cancellationToken);
if (!result.Success)
{
    // log warning, khong nen lam crash business flow
    // result.ErrorMessage co thong tin loi
}
```

### 6.3 Vi du gui mail text + Cc/Bcc

```csharp
using SHNGearMailService.Models;

var message = new EmailMessage
{
    Subject = "[SHNGear] Password Reset",
    Body = "Ma OTP cua ban la: 123456",
    IsHtml = false,
    To = { new EmailAddress { Address = "user@example.com" } },
    Cc = { new EmailAddress { Address = "support@example.com" } },
    Bcc = { new EmailAddress { Address = "audit@example.com" } }
};

var result = await _emailService.SendAsync(message, cancellationToken);
```

### 6.4 Vi du override nguoi gui cho 1 email

```csharp
using SHNGearMailService.Models;

var message = new EmailMessage
{
    From = new EmailAddress
    {
        Address = "campaign@shngear.com",
        DisplayName = "SHNGear Campaign"
    },
    Subject = "Special Offer",
    Body = "<p>Voucher 10% cho ban!</p>",
    IsHtml = true,
    To = { new EmailAddress { Address = "customer@example.com" } }
};

await _emailService.SendAsync(message, cancellationToken);
```

## 7. Hanh vi va validation hien tai

- Se fail neu:
  - Khong co nguoi nhan (`To` rong).
  - Subject rong.
  - Thieu cau hinh SMTP can thiet (`SmtpHost`, `Username`, `Password`, `FromAddress`).
- Timeout toi thieu la 5 giay.
- Loi gui mail duoc tra ve qua `EmailSendResult.Failed(...)`.

## 8. Tich hop de xuat trong SHNGearBE

- Auth flow:
  - Welcome email sau register.
  - Verify email / OTP email.
- Order flow:
  - Order confirmation sau checkout thanh cong.
  - Order status update notification.

Khuyen nghi: bat exception tai service layer va log warning de khong rollback nghiep vu chinh vi loi SMTP.

## 9. Docker env sample

```yaml
environment:
  - EMAIL_SMTP_HOST=smtp.gmail.com
  - EMAIL_SMTP_PORT=587
  - EMAIL_SMTP_USE_SSL=true
  - EMAIL_SMTP_USERNAME=noreply@your-domain.com
  - EMAIL_SMTP_PASSWORD=app-password
  - EMAIL_FROM_ADDRESS=noreply@your-domain.com
  - EMAIL_FROM_DISPLAY_NAME=SHNGear
  - EMAIL_TIMEOUT_SECONDS=30
```

## 10. Roadmap mo rong

- Them `IEmailTemplateRenderer` de tach template.
- Them provider strategy (SMTP/SendGrid/Mailgun).
- Them queue-based delivery (background worker) cho high throughput.

## 11. OTP auth flow (da tich hop)

Backend da co endpoint OTP cho xac nhan email va quen mat khau:

- `POST /api/Auth/send-verification-otp`
- `POST /api/Auth/verify-email-otp`
- `POST /api/Auth/forgot-password/send-otp`
- `POST /api/Auth/forgot-password/verify-otp`
- `POST /api/Auth/forgot-password/reset`

Vi du payload:

```json
{
  "email": "user@example.com"
}
```

```json
{
  "email": "user@example.com",
  "otp": "123456"
}
```

```json
{
  "email": "user@example.com",
  "verificationToken": "token-from-verify-step",
  "newPassword": "newStrongPassword"
}
```

### OTP template module

Mail service da tach rieng renderer cho OTP:

- `Abstractions/IEmailTemplateRenderer`
- `Templates/OtpEmailTemplateRenderer`

Example render + send:

```csharp
var rendered = _emailTemplateRenderer.RenderOtpTemplate(
  OtpEmailTemplateType.VerifyEmail,
  new OtpEmailTemplateData
  {
    RecipientName = "An Nguyen",
    OtpCode = "123456",
    ExpiryMinutes = 5
  });

await _emailService.SendAsync(new EmailMessage
{
  Subject = rendered.Subject,
  Body = rendered.HtmlBody,
  IsHtml = true,
  To = { new EmailAddress { Address = "an@example.com", DisplayName = "An Nguyen" } }
});
```
