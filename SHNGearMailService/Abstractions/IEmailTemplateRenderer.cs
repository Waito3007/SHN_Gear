using SHNGearMailService.Models;

namespace SHNGearMailService.Abstractions;

public interface IEmailTemplateRenderer
{
    RenderedEmailTemplate RenderOtpTemplate(OtpEmailTemplateType type, OtpEmailTemplateData data);
    RenderedEmailTemplate RenderOrderPlacedTemplate(OrderPlacedEmailTemplateData data);
    RenderedEmailTemplate RenderRefundCompletedTemplate(RefundCompletedEmailTemplateData data);
}
