using BackgroundLogService.Abstractions;
using System.Collections.Concurrent;

namespace BackgroundLogService.Services;

/// <summary>
/// Registry to track all LogService instances for flushing
/// </summary>
public interface ILogServiceRegistry
{
    void Register(IFlushableLogService service);
    void Unregister(IFlushableLogService service);
    IEnumerable<IFlushableLogService> GetAll();
}

/// <summary>
/// Interface for log services that can be flushed
/// </summary>
public interface IFlushableLogService
{
    Task ExecuteAsync();
}

/// <summary>
/// Singleton registry to track all LogService instances
/// </summary>
public class LogServiceRegistry : ILogServiceRegistry
{
    private readonly ConcurrentDictionary<IFlushableLogService, byte> _services = new();

    public void Register(IFlushableLogService service)
    {
        _services.TryAdd(service, 0);
    }

    public void Unregister(IFlushableLogService service)
    {
        _services.TryRemove(service, out _);
    }

    public IEnumerable<IFlushableLogService> GetAll()
    {
        return _services.Keys;
    }
}

