using BackgroundLogService.Models;

namespace BackgroundLogService.Abstractions;

/// <summary>
/// Abstraction for filtering sensitive data in logs
/// </summary>
public interface ILogFilter
{
    object? ApplyFilters(object? data, string sourceName);
    bool ShouldIgnoreMethod(string? method, string sourceName);
}

