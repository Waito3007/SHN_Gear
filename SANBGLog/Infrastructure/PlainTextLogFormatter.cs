using BackgroundLogService.Abstractions;
using BackgroundLogService.Models;
using System.Text;

namespace BackgroundLogService.Infrastructure;

/// <summary>
/// Plain text log formatter
/// </summary>
public class PlainTextLogFormatter : ILogFormatter
{
    private readonly IDateTimeProvider _dateTimeProvider;

    public PlainTextLogFormatter(IDateTimeProvider dateTimeProvider)
    {
        _dateTimeProvider = dateTimeProvider;
    }

    public string Format(LogEntry entry)
    {
        return entry.Type switch
        {
            LogEntryType.Message => FormatMessageEntry(entry),
            LogEntryType.Exception => FormatExceptionEntry(entry),
            LogEntryType.Data => FormatDataEntry(entry),
            _ => string.Empty
        };
    }

    public string FormatBatch(IEnumerable<LogEntry> entries)
    {
        var sb = new StringBuilder();
        foreach (var entry in entries)
        {
            sb.AppendLine(Format(entry));
        }
        return sb.ToString();
    }

    private static string FormatMessageEntry(LogEntry entry)
    {
        var category = string.IsNullOrEmpty(entry.Category) ? "" : $"[{entry.Category}] ";
        return $"[{entry.Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{entry.SessionLogId}] {category}[MSG] {entry.Message}";
    }

    private static string FormatExceptionEntry(LogEntry entry)
    {
        var category = string.IsNullOrEmpty(entry.Category) ? "" : $"[{entry.Category}] ";
        var sb = new StringBuilder();
        sb.AppendLine($"[{entry.Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{entry.SessionLogId}] {category}[ERR] {entry.ExceptionType}");
        sb.AppendLine($"  Message: {entry.Message}");
        sb.AppendLine($"  Method: {entry.Method}");
        sb.AppendLine($"  Source: {entry.Source}");
        if (!string.IsNullOrEmpty(entry.StackTrace))
        {
            sb.AppendLine($"  StackTrace: {entry.StackTrace}");
        }
        if (!string.IsNullOrEmpty(entry.EmbeddedData))
        {
            sb.AppendLine($"  EmbeddedData: {entry.EmbeddedData}");
        }
        return sb.ToString();
    }

    private static string FormatDataEntry(LogEntry entry)
    {
        var category = string.IsNullOrEmpty(entry.Category) ? "" : $"[{entry.Category}] ";
        var sb = new StringBuilder();
        sb.AppendLine($"[{entry.Timestamp:yyyy-MM-dd HH:mm:ss.fff}] [{entry.SessionLogId}] {category}[DATA] {entry.Method}");
        sb.AppendLine($"  Request: {entry.Request}");
        sb.AppendLine($"  Response: {entry.Response}");
        return sb.ToString();
    }
}

