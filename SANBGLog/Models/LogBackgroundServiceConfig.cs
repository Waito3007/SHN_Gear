namespace BackgroundLogService.Models;

public class BackgroundLogServiceConfig
{
    /// <summary>
    /// Project/Application name - used as source name for log files and filtering
    /// Example: "SANProductService"
    /// </summary>
    public string ProjectName { get; set; } = "DefaultProject";
    
    public string LogDirectory { get; set; } = "C:\\Logs";
    public long MaxFileSizeBytes { get; set; } = 4 * 1024 * 1024;
    public int FlushIntervalSeconds { get; set; } = 5;
    public Dictionary<string, FilterLogBySource> FilterLogBySources { get; set; } = new();
}

public class FilterLogBySource
{
    public List<string> IgnoreByMethodList { get; set; } = new();
    public List<FilterItem> FilterList { get; set; } = new();
}

public class FilterItem
{
    public List<string> PrototypeList { get; set; } = new();
    public FilterType Type { get; set; } = FilterType.Hidden;
    public string ReplaceBy { get; set; } = "********";
    public int Start { get; set; } = 0;
    public int End { get; set; } = 0;
    public int Length { get; set; } = 0;
    public string? Pattern { get; set; }
}

public enum FilterType
{
    Hidden = 1,
    PartHidden = 2,
    Json2Object = 3,
    Regex = 4,
    Array = 5
}
