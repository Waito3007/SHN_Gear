using BackgroundLogService.Services.Interfaces;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using BackgroundLogService.Models;

namespace BackgroundLogService.Services;

/// <summary>
/// Background hosted service that periodically flushes all log queues
/// Supports both IBackgroundLogService (legacy) and ILogService&lt;T&gt; (new)
/// </summary>
public class LogFlushingHostedService : BackgroundService
{
    private readonly IEnumerable<IBackgroundLogService> _legacyLogServices;
    private readonly ILogServiceRegistry _registry;
    private readonly ILogger<LogFlushingHostedService> _logger;
    private readonly TimeSpan _flushInterval;

    public LogFlushingHostedService(
        IEnumerable<IBackgroundLogService> legacyLogServices,
        ILogServiceRegistry registry,
        IOptions<BackgroundLogServiceConfig> config,
        ILogger<LogFlushingHostedService> logger)
    {
        _legacyLogServices = legacyLogServices;
        _registry = registry;
        _logger = logger;
        _flushInterval = TimeSpan.FromSeconds(config.Value.FlushIntervalSeconds);
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Log flushing service started with interval: {Interval}s", _flushInterval.TotalSeconds);
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await FlushAllLogsAsync();
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Error occurred while flushing logs");
            }

            await Task.Delay(_flushInterval, stoppingToken);
        }
    }

    private async Task FlushAllLogsAsync()
    {
        // Flush legacy IBackgroundLogService instances
        var legacyTasks = _legacyLogServices.Select(s => s.ExecuteAsync());
        
        // Flush new ILogService<T> instances via registry
        var registryTasks = _registry.GetAll().Select(s => s.ExecuteAsync());
        
        await Task.WhenAll(legacyTasks.Concat(registryTasks));
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Log flushing service stopping, performing final flush...");
        
        try
        {
            await FlushAllLogsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during final log flush");
        }
        
        await base.StopAsync(cancellationToken);
    }
}
