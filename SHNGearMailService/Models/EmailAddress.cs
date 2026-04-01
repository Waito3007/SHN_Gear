namespace SHNGearMailService.Models;

public sealed class EmailAddress
{
    public string Address { get; set; } = string.Empty;
    public string? DisplayName { get; set; }
}
