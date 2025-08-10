using Microsoft.Extensions.Configuration;
using SigmaProcessor.Config;

namespace SigmaProcessor.Utils;

public static class ConfigurationHelper
{
    public static IConfiguration BuildConfiguration()
    {
        var builder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        return builder.Build();
    }

    public static AppSettings GetAppSettings(IConfiguration configuration)
    {
        var appSettings = new AppSettings();
        configuration.Bind(appSettings);
        return appSettings;
    }

    public static void ValidateConfiguration(AppSettings appSettings)
    {
        if (string.IsNullOrWhiteSpace(appSettings.Api.BaseUrl))
        {
            throw new InvalidOperationException("API BaseUrl is required");
        }

        if (string.IsNullOrWhiteSpace(appSettings.Api.Username))
        {
            throw new InvalidOperationException("API Username is required");
        }

        if (string.IsNullOrWhiteSpace(appSettings.Api.Password))
        {
            throw new InvalidOperationException("API Password is required");
        }

        if (string.IsNullOrWhiteSpace(appSettings.Processing.InputPath))
        {
            throw new InvalidOperationException("Input path is required");
        }

        if (appSettings.Processing.BatchSize <= 0)
        {
            throw new InvalidOperationException("BatchSize must be greater than 0");
        }

        if (appSettings.Processing.MaxRetries < 0)
        {
            throw new InvalidOperationException("MaxRetries cannot be negative");
        }
    }
}
