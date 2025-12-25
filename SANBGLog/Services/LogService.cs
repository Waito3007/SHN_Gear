using BackgroundLogService.Abstractions;
using BackgroundLogService.Extensions;
using BackgroundLogService.Models;
using BackgroundLogService.Services.Interfaces;

namespace BackgroundLogService.Services;

/// <summary>
/// Modern log service with full dependency injection support
/// Single Responsibility: Only handles log entry creation and queuing
/// </summary>
public class LogService : IBackgroundLogService
{
    private readonly string _sourceName;
    private readonly string _categoryName;
    private readonly ILogQueue _logQueue;
    private readonly ILogQueue _dataQueue;
    private readonly ILogFilter _logFilter;
    private readonly ILogFormatter _logFormatter;
    private readonly ILogWriter _logWriter;
    private readonly IDateTimeProvider _dateTimeProvider;

    public LogService(
        string sourceName,
        string categoryName,
        ILogQueue logQueue,
        ILogQueue dataQueue,
        ILogFilter logFilter,
        ILogFormatter logFormatter,
        ILogWriter logWriter,
        IDateTimeProvider dateTimeProvider)
    {
        _sourceName = sourceName ?? throw new ArgumentNullException(nameof(sourceName));
        _categoryName = categoryName ?? sourceName; // Mặc định là sourceName nếu không có categoryName
        _logQueue = logQueue ?? throw new ArgumentNullException(nameof(logQueue));
        _dataQueue = dataQueue ?? throw new ArgumentNullException(nameof(dataQueue));
        _logFilter = logFilter ?? throw new ArgumentNullException(nameof(logFilter));
        _logFormatter = logFormatter ?? throw new ArgumentNullException(nameof(logFormatter));
        _logWriter = logWriter ?? throw new ArgumentNullException(nameof(logWriter));
        _dateTimeProvider = dateTimeProvider ?? throw new ArgumentNullException(nameof(dateTimeProvider));
    }

    public async Task ExecuteAsync()
    {
        await FlushQueueAsync(_logQueue, LogOutputType.Log);
        await FlushQueueAsync(_dataQueue, LogOutputType.Data);
    }

    public Task WriteMessageAsync(ISessionLogService? sessionLogService, string? message)
    {
        var entry = new LogEntry
        {
            Type = LogEntryType.Message,
            Timestamp = _dateTimeProvider.Now,
            SessionLogId = sessionLogService?.GetSessionLogId() ?? "NO-SESSION",
            Message = message
        };
        _logQueue.Enqueue(entry);
        return Task.CompletedTask;
    }

    public Task WriteExceptionAsync(ISessionLogService? sessionLogService, Exception ex)
    {
        var embeddedData = ex is CustomException customEx ? customEx.EmbeddedData : null;
        
        var entry = new LogEntry
        {
            Type = LogEntryType.Exception,
            Timestamp = _dateTimeProvider.Now,
            SessionLogId = sessionLogService?.GetSessionLogId() ?? "NO-SESSION",
            ExceptionType = ex.GetType().Name,
            Message = ex.Message,
            Source = ex.Source,
            StackTrace = ex.StackTrace,
            Method = ex.TargetSite?.DeclaringType?.Name + "." + ex.TargetSite?.Name,
            EmbeddedData = embeddedData?.ToJson()
        };
        _logQueue.Enqueue(entry);
        return Task.CompletedTask;
    }

    public Task WriteLogDataAsync(ISessionLogService? sessionLogService, string? method, object? request, object? response)
    {
        if (_logFilter.ShouldIgnoreMethod(method, _sourceName))
        {
            return Task.CompletedTask;
        }

        var filteredRequest = _logFilter.ApplyFilters(request, _sourceName);
        var filteredResponse = _logFilter.ApplyFilters(response, _sourceName);

        var entry = new LogEntry
        {
            Type = LogEntryType.Data,
            Timestamp = _dateTimeProvider.Now,
            SessionLogId = sessionLogService?.GetSessionLogId() ?? "NO-SESSION",
            Method = method,
            Request = filteredRequest,
            Response = filteredResponse
        };
        _dataQueue.Enqueue(entry);
        return Task.CompletedTask;
    }

    public bool EqualsWithName(string sourceName)
    {
        return string.Equals(_sourceName, sourceName, StringComparison.OrdinalIgnoreCase);
    }

    public string GetPartnerCode() => _sourceName;

    private async Task FlushQueueAsync(ILogQueue queue, LogOutputType outputType)
    {
        var entries = queue.DequeueAll();
        if (entries.Count == 0) return;

        var content = _logFormatter.FormatBatch(entries);
        await _logWriter.WriteAsync(_sourceName, _categoryName, content, outputType);
    }
}

