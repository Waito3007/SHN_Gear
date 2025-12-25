using BackgroundLogService.Abstractions;
using BackgroundLogService.Models;
using Microsoft.Extensions.Options;

namespace BackgroundLogService.Infrastructure;

/// <summary>
/// File-based log writer with automatic file rotation
/// </summary>
public class FileLogWriter : ILogWriter
{
    private readonly BackgroundLogServiceConfig _config;
    private readonly IDateTimeProvider _dateTimeProvider;
    private readonly object _fileLock = new();
    
    private readonly Dictionary<string, FileWriterState> _writerStates = new();

    public FileLogWriter(
        IOptions<BackgroundLogServiceConfig> config,
        IDateTimeProvider dateTimeProvider)
    {
        _config = config.Value;
        _dateTimeProvider = dateTimeProvider;
    }

    public async Task WriteAsync(string sourceName, string categoryName, string content, LogOutputType outputType, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrEmpty(content)) return;

        // Sử dụng categoryName (tên class) làm key để quản lý state riêng cho mỗi class
        var stateKey = $"{sourceName}_{categoryName}";
        var state = GetOrCreateState(stateKey);
        var today = _dateTimeProvider.GetLogDateFormat();
        
        lock (_fileLock)
        {
            if (state.CurrentLogDate != today)
            {
                state.CurrentLogDate = today;
                state.LogIndex = 1;
                state.DataIndex = 1;
            }
        }

        // Tạo thư mục theo cấu trúc: LogDirectory/SourceName/CategoryName/
        // Ví dụ: C:\Logs\SANProductService\BrandService\
        var directory = Path.Combine(_config.LogDirectory, sourceName, categoryName);
        Directory.CreateDirectory(directory);

        var fileType = outputType == LogOutputType.Data ? "Data" : "Log";
        var index = outputType == LogOutputType.Data ? state.DataIndex : state.LogIndex;
        // Đặt tên file theo categoryName (tên class)
        var filePath = GetFilePath(directory, categoryName, fileType, today, index);

        // Check file size and rotate if needed
        if (File.Exists(filePath))
        {
            var fileInfo = new FileInfo(filePath);
            if (fileInfo.Length >= _config.MaxFileSizeBytes)
            {
                lock (_fileLock)
                {
                    if (outputType == LogOutputType.Data)
                        state.DataIndex++;
                    else
                        state.LogIndex++;
                }
                index = outputType == LogOutputType.Data ? state.DataIndex : state.LogIndex;
                filePath = GetFilePath(directory, categoryName, fileType, today, index);
            }
        }

        await File.AppendAllTextAsync(filePath, content, cancellationToken);
    }

    private FileWriterState GetOrCreateState(string sourceName)
    {
        lock (_fileLock)
        {
            if (!_writerStates.TryGetValue(sourceName, out var state))
            {
                state = new FileWriterState();
                _writerStates[sourceName] = state;
            }
            return state;
        }
    }

    private static string GetFilePath(string directory, string sourceName, string fileType, string date, int index)
    {
        return Path.Combine(directory, $"{sourceName}_{fileType}_{date}_{index:D3}.txt");
    }

    private class FileWriterState
    {
        public string CurrentLogDate { get; set; } = "";
        public int LogIndex { get; set; } = 1;
        public int DataIndex { get; set; } = 1;
    }
}

