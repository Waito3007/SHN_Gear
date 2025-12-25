using BackgroundLogService.Services;
using BackgroundLogService.Services.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace BackgroundLogService.Extensions;

public static class LogExtensionLibrary
{
    public static IBackgroundLogService? GetLogService(this IServiceProvider serviceProvider, string sourceName)
    {
        var logServices = serviceProvider.GetServices<IBackgroundLogService>();
        return logServices.FirstOrDefault(s => s.EqualsWithName(sourceName));
    }

    public static ISessionLogService GetSessionLogService(this IServiceProvider serviceProvider)
    {
        return serviceProvider.GetRequiredService<ISessionLogService>();
    }

    public static ISessionLogService GetNewSessionLogService(this IServiceProvider serviceProvider)
    {
        var sessionService = serviceProvider.GetRequiredService<ISessionLogService>();
        sessionService.SetSessionLogId(Guid.NewGuid().ToString("N")[..12]);
        return sessionService;
    }
}
