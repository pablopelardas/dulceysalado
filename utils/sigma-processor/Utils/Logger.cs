using Microsoft.Extensions.Logging;

namespace SigmaProcessor.Utils;

public static class Logger
{
    public static ILoggerFactory CreateLoggerFactory()
    {
        return LoggerFactory.Create(builder =>
        {
            builder
                .AddConsole()
                .SetMinimumLevel(LogLevel.Information);
        });
    }

    public static void LogProcessingStart(ILogger logger, string fileName)
    {
        logger.LogInformation("=== INICIANDO PROCESAMIENTO ===");
        logger.LogInformation("Archivo: {FileName}", fileName);
        logger.LogInformation("Fecha: {DateTime}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
        logger.LogInformation("================================");
    }

    public static void LogProcessingEnd(ILogger logger, string fileName, int nuevos, int actualizados, int errores, long timeMs)
    {
        logger.LogInformation("=== PROCESAMIENTO COMPLETADO ===");
        logger.LogInformation("Archivo: {FileName}", fileName);
        logger.LogInformation("Productos nuevos: {Nuevos}", nuevos);
        logger.LogInformation("Productos actualizados: {Actualizados}", actualizados);
        logger.LogInformation("Errores: {Errores}", errores);
        logger.LogInformation("Tiempo total: {TimeMs} ms", timeMs);
        logger.LogInformation("================================");
    }

    public static void LogError(ILogger logger, Exception ex, string context)
    {
        logger.LogError(ex, "ERROR en {Context}: {Message}", context, ex.Message);
    }

    public static void LogProgress(ILogger logger, int processed, int total)
    {
        var percentage = (double)processed / total * 100;
        logger.LogInformation("Progreso: {Processed}/{Total} ({Percentage:F1}%)", processed, total, percentage);
    }
}