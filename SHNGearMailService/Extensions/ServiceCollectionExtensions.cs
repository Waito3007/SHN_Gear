using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SHNGearMailService.Abstractions;
using SHNGearMailService.Infrastructure;
using SHNGearMailService.Models;
using SHNGearMailService.Templates;

namespace SHNGearMailService.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddMailService(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<EmailServiceSettings>(options =>
        {
            configuration.GetSection(EmailServiceSettings.SectionName).Bind(options);

            options.SmtpHost = FirstNonEmpty(
                options.SmtpHost,
                Environment.GetEnvironmentVariable("EMAIL_SMTP_HOST"),
                Environment.GetEnvironmentVariable("SMTP_HOST"));
            options.SmtpPort = options.SmtpPort <= 0
                ? ParseInt(
                    Environment.GetEnvironmentVariable("EMAIL_SMTP_PORT"),
                    Environment.GetEnvironmentVariable("SMTP_PORT"),
                    587)
                : options.SmtpPort;
            options.UseSsl = ParseBool(Environment.GetEnvironmentVariable("EMAIL_SMTP_USE_SSL"), options.UseSsl);
            options.Username = FirstNonEmpty(
                options.Username,
                Environment.GetEnvironmentVariable("EMAIL_SMTP_USERNAME"),
                Environment.GetEnvironmentVariable("SENDER_EMAIL"));
            options.Password = FirstNonEmpty(
                options.Password,
                Environment.GetEnvironmentVariable("EMAIL_SMTP_PASSWORD"),
                Environment.GetEnvironmentVariable("SENDER_PASSWORD"));
            options.FromAddress = FirstNonEmpty(
                options.FromAddress,
                Environment.GetEnvironmentVariable("EMAIL_FROM_ADDRESS"),
                Environment.GetEnvironmentVariable("SENDER_EMAIL"));
            options.FromDisplayName = FirstNonEmpty(options.FromDisplayName, Environment.GetEnvironmentVariable("EMAIL_FROM_DISPLAY_NAME"));
            options.TimeoutSeconds = options.TimeoutSeconds <= 0
                ? ParseInt(Environment.GetEnvironmentVariable("EMAIL_TIMEOUT_SECONDS"), null, 30)
                : options.TimeoutSeconds;
        });

        services.AddScoped<IEmailService, SmtpEmailService>();
        services.AddScoped<IEmailTemplateRenderer, OtpEmailTemplateRenderer>();
        return services;
    }

    private static string FirstNonEmpty(params string?[] values)
    {
        foreach (var value in values)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value;
            }
        }

        return string.Empty;
    }

    private static int ParseInt(string? first, string? second, int fallback)
    {
        if (int.TryParse(first, out var firstParsed))
        {
            return firstParsed;
        }

        if (int.TryParse(second, out var secondParsed))
        {
            return secondParsed;
        }

        return fallback;
    }

    private static bool ParseBool(string? raw, bool fallback)
    {
        return bool.TryParse(raw, out var parsed) ? parsed : fallback;
    }
}
