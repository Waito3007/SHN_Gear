using BackgroundLogService.Abstractions;
using BackgroundLogService.Infrastructure;
using BackgroundLogService.Services.Interfaces;
using Microsoft.Extensions.Options;

namespace BackgroundLogService.Services;

/// <summary>
/// Factory for creating LogService instances with proper dependencies
/// Enables easy testing by allowing mock dependencies
/// </summary>
public interface ILogServiceFactory
{
    IBackgroundLogService Create(string sourceName);
    IBackgroundLogService Create(string sourceName, string categoryName);
}

public class LogServiceFactory : ILogServiceFactory
{
    private readonly ILogFilter _logFilter;
    private readonly ILogFormatter _logFormatter;
    private readonly ILogWriter _logWriter;
    private readonly IDateTimeProvider _dateTimeProvider;

    public LogServiceFactory(
        ILogFilter logFilter,
        ILogFormatter logFormatter,
        ILogWriter logWriter,
        IDateTimeProvider dateTimeProvider)
    {
        _logFilter = logFilter;
        _logFormatter = logFormatter;
        _logWriter = logWriter;
        _dateTimeProvider = dateTimeProvider;
    }

    public IBackgroundLogService Create(string sourceName)
    {
        // Default: categoryName = sourceName for backward compatibility
        return Create(sourceName, sourceName);
    }

    public IBackgroundLogService Create(string sourceName, string categoryName)
    {
        // Each service gets its own queues for isolation
        var logQueue = new InMemoryLogQueue();
        var dataQueue = new InMemoryLogQueue();

        return new LogService(
            sourceName,
            categoryName,
            logQueue,
            dataQueue,
            _logFilter,
            _logFormatter,
            _logWriter,
            _dateTimeProvider);
    }
}

