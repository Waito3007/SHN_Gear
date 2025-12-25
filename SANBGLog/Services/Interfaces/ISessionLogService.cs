namespace BackgroundLogService.Services.Interfaces;

public interface ISessionLogService
{
    string GetSessionLogId();
    void SetSessionLogId(string sessionLogId);
    bool EqualsWithName(string sessionLogId);
}
