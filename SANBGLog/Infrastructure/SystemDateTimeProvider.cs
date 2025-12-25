using BackgroundLogService.Abstractions;
using BackgroundLogService.Extensions;

namespace BackgroundLogService.Infrastructure;

/// <summary>
/// Default implementation of IDateTimeProvider
/// </summary>
public class SystemDateTimeProvider : IDateTimeProvider
{
    public DateTime Now => DateTime.Now;
    public DateTime UtcNow => DateTime.UtcNow;
    public string GetLogDateFormat() => DateTime.Now.ToLogDateFormat();
}