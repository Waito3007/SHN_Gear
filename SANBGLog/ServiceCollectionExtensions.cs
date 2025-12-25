using BackgroundLogService.Abstractions;
using BackgroundLogService.Infrastructure;
using BackgroundLogService.Models;
using BackgroundLogService.Services;
using BackgroundLogService.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace BackgroundLogService;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBackgroundLogService(
        this IServiceCollection services,
        IConfiguration configuration,
        params string[] sourceNames)
    {
        services.Configure<BackgroundLogServiceConfig>(
            configuration.GetSection("BackgroundLogServiceConfig"));

        services.TryAddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.TryAddSingleton<ILogWriter, FileLogWriter>();
        services.TryAddSingleton<ILogFormatter, PlainTextLogFormatter>();
        services.TryAddSingleton<ILogFilter, DefaultLogFilter>();
        services.TryAddSingleton<ILogServiceFactory, LogServiceFactory>();
        services.TryAddSingleton<ILogServiceRegistry, LogServiceRegistry>();

        services.AddScoped<ISessionLogService, SessionLogService>();

        services.AddScoped(typeof(ILogService<>), typeof(LogService<>));

        foreach (var sourceName in sourceNames)
        {
            var name = sourceName;
            services.AddSingleton<IBackgroundLogService>(provider =>
            {
                var factory = provider.GetRequiredService<ILogServiceFactory>();
                return factory.Create(name);
            });
        }

        services.AddHostedService<LogFlushingHostedService>();

        return services;
    }

    public static IServiceCollection AddBackgroundLogService(
        this IServiceCollection services,
        IConfiguration configuration,
        string sourceName)
    {
        return services.AddBackgroundLogService(configuration, new[] { sourceName });
    }

    public static IServiceCollection AddTypedLogService(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<BackgroundLogServiceConfig>(
            configuration.GetSection("BackgroundLogServiceConfig"));

        services.TryAddSingleton<IDateTimeProvider, SystemDateTimeProvider>();
        services.TryAddSingleton<ILogWriter, FileLogWriter>();
        services.TryAddSingleton<ILogFormatter, PlainTextLogFormatter>();
        services.TryAddSingleton<ILogFilter, DefaultLogFilter>();
        services.TryAddSingleton<ILogServiceFactory, LogServiceFactory>();
        services.TryAddSingleton<ILogServiceRegistry, LogServiceRegistry>();

        services.AddScoped<ISessionLogService, SessionLogService>();

        services.AddScoped(typeof(ILogService<>), typeof(LogService<>));

        services.AddHostedService<LogFlushingHostedService>();

        return services;
    }

    public static IServiceCollection AddBackgroundLogService(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<LogServiceBuilder> configure)
    {
        var builder = new LogServiceBuilder(services, configuration);
        configure(builder);
        return builder.Build();
    }
}

public class LogServiceBuilder
{
    private readonly IServiceCollection _services;
    private readonly IConfiguration _configuration;
    private readonly List<string> _sourceNames = new();

    public LogServiceBuilder(IServiceCollection services, IConfiguration configuration)
    {
        _services = services;
        _configuration = configuration;
    }

    public LogServiceBuilder AddSource(string sourceName)
    {
        _sourceNames.Add(sourceName);
        return this;
    }

    public LogServiceBuilder AddSources(params string[] sourceNames)
    {
        _sourceNames.AddRange(sourceNames);
        return this;
    }

    public LogServiceBuilder UseLogWriter<TLogWriter>() where TLogWriter : class, ILogWriter
    {
        _services.AddSingleton<ILogWriter, TLogWriter>();
        return this;
    }

    public LogServiceBuilder UseLogFormatter<TFormatter>() where TFormatter : class, ILogFormatter
    {
        _services.AddSingleton<ILogFormatter, TFormatter>();
        return this;
    }

    public LogServiceBuilder UseLogFilter<TFilter>() where TFilter : class, ILogFilter
    {
        _services.AddSingleton<ILogFilter, TFilter>();
        return this;
    }

    public LogServiceBuilder UseDateTimeProvider<TProvider>() where TProvider : class, IDateTimeProvider
    {
        _services.AddSingleton<IDateTimeProvider, TProvider>();
        return this;
    }

    public IServiceCollection Build()
    {
        return _services.AddBackgroundLogService(_configuration, _sourceNames.ToArray());
    }
}
