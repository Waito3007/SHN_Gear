using BackgroundLogService.Abstractions;
using BackgroundLogService.Extensions;
using BackgroundLogService.Infrastructure;
using BackgroundLogService.Models;
using BackgroundLogService.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace BackgroundLogService.Services;

/// <summary>
/// Typed log service that can be injected directly via DI
/// Usage: ILogService&lt;BrandService&gt; logService
/// SourceName = ProjectName from config (e.g., "SANProductService")
/// CategoryName = Class name (e.g., "BrandService")
/// </summary>
public class LogService<TCategory> : ILogService<TCategory>, IFlushableLogService, IDisposable
    where TCategory : class
{
    private readonly ILogQueue _logQueue;
    private readonly ILogQueue _dataQueue;
    private readonly ILogFilter _logFilter;
    private readonly ILogFormatter _logFormatter;
    private readonly ILogWriter _logWriter;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly ISessionLogService _sessionLogService;
    private readonly ILogServiceRegistry _registry;

    /// <summary>
    /// Project name from config - used for file path and filtering
    /// </summary>
    public string SourceName { get; }

    /// <summary>
    /// Class name - used for log identification
    /// </summary>
    public string CategoryName { get; }

    public LogService(
        IOptions<BackgroundLogServiceConfig> config,
        ILogFilter logFilter,
        ILogFormatter logFormatter,
        ILogWriter logWriter,
        IDateTimeProvider dateTimeProvider,
        ISessionLogService sessionLogService,
        ILogServiceRegistry registry)
    {
        SourceName = config.Value.ProjectName;  // From appsettings.json
        CategoryName = typeof(TCategory).Name;   // Class name for identification
        _logQueue = new InMemoryLogQueue();
        _dataQueue = new InMemoryLogQueue();
        _logFilter = logFilter;
        _logFormatter = logFormatter;
        _logWriter = logWriter;
        _dateTimeProvider = dateTimeProvider;
        _sessionLogService = sessionLogService;
        _registry = registry;

        // Register this instance for flushing
        _registry.Register(this);
    }

    public Task WriteMessageAsync(string? message)
    {
        var entry = new LogEntry
        {
            Type = LogEntryType.Message,
            Timestamp = _dateTimeProvider.Now,
            SessionLogId = _sessionLogService.GetSessionLogId(),
            Category = CategoryName,
            Message = message
        };
        _logQueue.Enqueue(entry);
        return Task.CompletedTask;
    }

    public Task WriteExceptionAsync(Exception ex)
    {
        var embeddedData = ex is CustomException customEx ? customEx.EmbeddedData : null;

        var entry = new LogEntry
        {
            Type = LogEntryType.Exception,
            Timestamp = _dateTimeProvider.Now,
            SessionLogId = _sessionLogService.GetSessionLogId(),
            Category = CategoryName,
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

    public Task WriteLogDataAsync(string? method, object? request, object? response)
    {
        if (_logFilter.ShouldIgnoreMethod(method, SourceName))
        {
            return Task.CompletedTask;
        }

        var filteredRequest = _logFilter.ApplyFilters(request, SourceName);
        var filteredResponse = _logFilter.ApplyFilters(response, SourceName);

        var entry = new LogEntry
        {
            Type = LogEntryType.Data,
            Timestamp = _dateTimeProvider.Now,
            SessionLogId = _sessionLogService.GetSessionLogId(),
            Category = CategoryName,
            Method = method,
            Request = filteredRequest,
            Response = filteredResponse
        };
        _dataQueue.Enqueue(entry);
        return Task.CompletedTask;
    }

    public async Task ExecuteAsync()
    {
        await FlushQueueAsync(_logQueue, LogOutputType.Log);
        await FlushQueueAsync(_dataQueue, LogOutputType.Data);
    }

    private async Task FlushQueueAsync(ILogQueue queue, LogOutputType outputType)
    {
        var entries = queue.DequeueAll();
        if (entries.Count == 0) return;

        var content = _logFormatter.FormatBatch(entries);
        await _logWriter.WriteAsync(SourceName, CategoryName, content, outputType);
    }

    public void Dispose()
    {
        ExecuteAsync().GetAwaiter().GetResult();
        _registry.Unregister(this);
    }
}

