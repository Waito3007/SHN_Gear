namespace SHNGearMailService.Models;

public sealed class EmailServiceSettings
{
    public const string SectionName = "EmailService";

    public string Provider { get; set; } = "Smtp";
    public string SmtpHost { get; set; } = string.Empty;
    public int SmtpPort { get; set; } = 587;
    public bool UseSsl { get; set; } = true;
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string FromAddress { get; set; } = string.Empty;
    public string FromDisplayName { get; set; } = "SHNGear";
    public int TimeoutSeconds { get; set; } = 30;
}
