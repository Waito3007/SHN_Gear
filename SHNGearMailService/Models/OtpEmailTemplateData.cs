namespace SHNGearMailService.Models;

public sealed class OtpEmailTemplateData
{
    public string AppName { get; set; } = "SHNGear";
    public string RecipientName { get; set; } = "User";
    public string OtpCode { get; set; } = string.Empty;
    public int ExpiryMinutes { get; set; } = 5;
    public string SupportEmail { get; set; } = "support@shngear.com";
}
