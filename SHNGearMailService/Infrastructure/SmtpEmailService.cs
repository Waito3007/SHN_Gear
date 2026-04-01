using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using SHNGearMailService.Abstractions;
using SHNGearMailService.Models;

namespace SHNGearMailService.Infrastructure;

public sealed class SmtpEmailService : IEmailService
{
    private readonly EmailServiceSettings _settings;

    public SmtpEmailService(IOptions<EmailServiceSettings> settings)
    {
        _settings = settings.Value;
    }

    public async Task<EmailSendResult> SendAsync(EmailMessage message, CancellationToken cancellationToken = default)
    {
        if (message.To.Count == 0)
        {
            return EmailSendResult.Failed("At least one recipient is required.");
        }

        if (string.IsNullOrWhiteSpace(message.Subject))
        {
            return EmailSendResult.Failed("Email subject cannot be empty.");
        }

        if (string.IsNullOrWhiteSpace(_settings.SmtpHost)
            || string.IsNullOrWhiteSpace(_settings.FromAddress)
            )
        {
            return EmailSendResult.Failed("Email service is not configured.");
        }

        using var mailMessage = BuildMailMessage(message);
        using var smtpClient = BuildSmtpClient();

        try
        {
            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(Math.Max(5, _settings.TimeoutSeconds)));
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);
            await smtpClient.SendMailAsync(mailMessage, linkedCts.Token);
            return EmailSendResult.Ok();
        }
        catch (Exception ex)
        {
            return EmailSendResult.Failed(ex.Message);
        }
    }

    private MailMessage BuildMailMessage(EmailMessage message)
    {
        var from = message.From?.Address;
        var displayName = message.From?.DisplayName;

        if (string.IsNullOrWhiteSpace(from))
        {
            from = _settings.FromAddress;
            displayName = _settings.FromDisplayName;
        }

        var mailMessage = new MailMessage
        {
            From = new MailAddress(from, displayName),
            Subject = message.Subject,
            Body = message.Body,
            IsBodyHtml = message.IsHtml
        };

        foreach (var to in message.To)
        {
            if (!string.IsNullOrWhiteSpace(to.Address))
            {
                mailMessage.To.Add(new MailAddress(to.Address, to.DisplayName));
            }
        }

        foreach (var cc in message.Cc)
        {
            if (!string.IsNullOrWhiteSpace(cc.Address))
            {
                mailMessage.CC.Add(new MailAddress(cc.Address, cc.DisplayName));
            }
        }

        foreach (var bcc in message.Bcc)
        {
            if (!string.IsNullOrWhiteSpace(bcc.Address))
            {
                mailMessage.Bcc.Add(new MailAddress(bcc.Address, bcc.DisplayName));
            }
        }

        return mailMessage;
    }

    private SmtpClient BuildSmtpClient()
    {
        var smtpClient = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
        {
            EnableSsl = _settings.UseSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            Timeout = Math.Max(5000, _settings.TimeoutSeconds * 1000)
        };

        // Allow SMTP without auth for local relay tools (e.g. Mailpit/Mailhog).
        if (!string.IsNullOrWhiteSpace(_settings.Username) && !string.IsNullOrWhiteSpace(_settings.Password))
        {
            smtpClient.Credentials = new NetworkCredential(_settings.Username, _settings.Password);
        }

        return smtpClient;
    }
}
