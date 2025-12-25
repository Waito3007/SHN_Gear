using BackgroundLogService.Models;

namespace BackgroundLogService.Abstractions;

/// <summary>
/// Abstraction for formatting log entries
/// Allows different formatting strategies (JSON, plain text, structured, etc.)
/// </summary>
public interface ILogFormatter
{
    string Format(LogEntry entry);
    string FormatBatch(IEnumerable<LogEntry> entries);
}

