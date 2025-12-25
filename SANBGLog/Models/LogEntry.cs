namespace BackgroundLogService.Models;

/// <summary>
/// Represents a single log entry
/// </summary>
public class LogEntry
{
    public LogEntryType Type { get; set; }
    public DateTime Timestamp { get; set; }
    public string SessionLogId { get; set; } = string.Empty;
    
    /// <summary>
    /// Category/Class name (e.g., "BrandService", "OrderService")
    /// </summary>
    public string? Category { get; set; }
    
    public string? Message { get; set; }
    public string? ExceptionType { get; set; }
    public string? Source { get; set; }
    public string? StackTrace { get; set; }
    public string? Method { get; set; }
    public string? EmbeddedData { get; set; }
    public object? Request { get; set; }
    public object? Response { get; set; }
}

public enum LogEntryType
{
    Message = 1,
    Exception = 2,
    Data = 3
}

