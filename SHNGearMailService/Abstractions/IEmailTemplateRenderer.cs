using SHNGearMailService.Models;

namespace SHNGearMailService.Abstractions;

public interface IEmailTemplateRenderer
{
    RenderedEmailTemplate RenderOtpTemplate(OtpEmailTemplateType type, OtpEmailTemplateData data);
}
