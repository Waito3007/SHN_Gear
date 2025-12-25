using BackgroundLogService.Abstractions;
using BackgroundLogService.Models;
using System.Collections.Concurrent;

namespace BackgroundLogService.Infrastructure;

/// <summary>
/// Thread-safe in-memory log queue implementation
/// </summary>
public class InMemoryLogQueue : ILogQueue
{
    private readonly ConcurrentQueue<LogEntry> _queue = new();

    public int Count => _queue.Count;
    public bool IsEmpty => _queue.IsEmpty;

    public void Enqueue(LogEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        _queue.Enqueue(entry);
    }

    public IReadOnlyList<LogEntry> DequeueAll()
    {
        var entries = new List<LogEntry>();
        while (_queue.TryDequeue(out var entry))
        {
            entries.Add(entry);
        }
        return entries;
    }
}

