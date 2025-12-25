namespace BackgroundLogService.Abstractions;

/// <summary>
/// Generic interface for typed log service injection
/// Allows direct DI injection: ILogService&lt;BrandService&gt;
/// </summary>
/// <typeparam name="TCategory">The type used for categorizing logs (e.g., BrandService, OrderService)</typeparam>
public interface ILogService<TCategory> where TCategory : class
{
    /// <summary>
    /// Gets the project/source name (from config) - used for file path and filtering
    /// </summary>
    string SourceName { get; }
    
    /// <summary>
    /// Gets the category name (class name) - used for log identification
    /// </summary>
    string CategoryName { get; }
    
    /// <summary>
    /// Write a message to the log
    /// </summary>
    Task WriteMessageAsync(string? message);
    
    /// <summary>
    /// Write an exception to the log
    /// </summary>
    Task WriteExceptionAsync(Exception ex);
    
    /// <summary>
    /// Write request/response data to the log
    /// </summary>
    Task WriteLogDataAsync(string? method, object? request, object? response);
    
    /// <summary>
    /// Execute flush operation
    /// </summary>
    Task ExecuteAsync();
}

