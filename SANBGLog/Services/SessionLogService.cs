using BackgroundLogService.Services.Interfaces;

namespace BackgroundLogService.Services;

public class SessionLogService : ISessionLogService
{
    private string _sessionLogId;

    public SessionLogService()
    {
        _sessionLogId = Guid.NewGuid().ToString("N")[..12];
    }

    public string GetSessionLogId()
    {
        return _sessionLogId;
    }

    public void SetSessionLogId(string sessionLogId)
    {
        _sessionLogId = sessionLogId ?? Guid.NewGuid().ToString("N")[..12];
    }

    public bool EqualsWithName(string sessionLogId)
    {
        return string.Equals(_sessionLogId, sessionLogId, StringComparison.OrdinalIgnoreCase);
    }
}
