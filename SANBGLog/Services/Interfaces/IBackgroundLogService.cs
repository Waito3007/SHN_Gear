namespace BackgroundLogService.Services.Interfaces;

public interface IBackgroundLogService
{
    Task ExecuteAsync();
    Task WriteMessageAsync(ISessionLogService? sessionLogService, string? message);
    Task WriteExceptionAsync(ISessionLogService? sessionLogService, Exception ex);
    Task WriteLogDataAsync(ISessionLogService? sessionLogService, string? method, object? request, object? response);
    bool EqualsWithName(string sourceName);
    string GetPartnerCode();
}
