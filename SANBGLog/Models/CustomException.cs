namespace BackgroundLogService.Models;

public class CustomException : Exception
{
    public Dictionary<string, object?> EmbeddedData { get; set; } = new();

    public CustomException() : base()
    {
    }

    public CustomException(string message) : base(message)
    {
    }

    public CustomException(string message, Exception innerException) : base(message, innerException)
    {
    }

    public CustomException(string message, Dictionary<string, object?> embeddedData) : base(message)
    {
        EmbeddedData = embeddedData;
    }

    public CustomException(string message, Dictionary<string, object?> embeddedData, Exception innerException) 
        : base(message, innerException)
    {
        EmbeddedData = embeddedData;
    }

    public CustomException WithData(string key, object? value)
    {
        EmbeddedData[key] = value;
        return this;
    }

    public CustomException WithData(Dictionary<string, object?> data)
    {
        foreach (var kvp in data)
        {
            EmbeddedData[kvp.Key] = kvp.Value;
        }
        return this;
    }
}
