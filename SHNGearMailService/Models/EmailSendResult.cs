namespace SHNGearMailService.Models;

public sealed class EmailSendResult
{
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }

    public static EmailSendResult Ok() => new() { Success = true };

    public static EmailSendResult Failed(string errorMessage) => new()
    {
        Success = false,
        ErrorMessage = errorMessage
    };
}
