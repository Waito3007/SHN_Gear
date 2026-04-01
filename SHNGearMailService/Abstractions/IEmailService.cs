using SHNGearMailService.Models;

namespace SHNGearMailService.Abstractions;

public interface IEmailService
{
    Task<EmailSendResult> SendAsync(EmailMessage message, CancellationToken cancellationToken = default);
}
