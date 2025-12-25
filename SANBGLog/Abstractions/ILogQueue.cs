using BackgroundLogService.Models;

namespace BackgroundLogService.Abstractions;

/// <summary>
/// Abstraction for log queue operations
/// Enables testing and alternative queue implementations
/// </summary>
public interface ILogQueue
{
    void Enqueue(LogEntry entry);
    IReadOnlyList<LogEntry> DequeueAll();
    int Count { get; }
    bool IsEmpty { get; }
}

