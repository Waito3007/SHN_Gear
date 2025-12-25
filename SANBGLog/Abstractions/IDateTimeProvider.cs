namespace BackgroundLogService.Abstractions;

/// <summary>
/// Abstraction for datetime operations - enables testing with fixed time
/// </summary>
public interface IDateTimeProvider
{
    DateTime Now { get; }
    DateTime UtcNow { get; }
    string GetLogDateFormat();
}


