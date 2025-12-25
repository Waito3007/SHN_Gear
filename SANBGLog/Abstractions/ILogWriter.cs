namespace BackgroundLogService.Abstractions;

/// <summary>
/// Abstraction for writing logs to different destinations (file, database, cloud, etc.)
/// This allows easy testing with mock implementations
/// </summary>
public interface ILogWriter
{
    Task WriteAsync(string sourceName, string categoryName, string content, LogOutputType outputType, CancellationToken cancellationToken = default);
}

public enum LogOutputType
{
    Log,
    Data
}