using System.Text.Json;
using Microsoft.Extensions.Logging;
using SigmaProcessor.Config;

namespace SigmaProcessor.Services;

public class FileLoggerService
{
    private readonly ProcessingConfig _processingConfig;
    private readonly ILogger<FileLoggerService> _logger;
    private readonly string _logDirectory;

    public FileLoggerService(ProcessingConfig processingConfig, ILogger<FileLoggerService> logger)
    {
        _processingConfig = processingConfig;
        _logger = logger;
        _logDirectory = Path.Combine(processingConfig.TempPath, "logs");
        
        // Asegurar que el directorio de logs existe
        if (!Directory.Exists(_logDirectory))
        {
            Directory.CreateDirectory(_logDirectory);
        }
    }

    public async Task LogProcessingCompletedAsync(string sessionId, BatchProcessingSummary summary)
    {
        var logEntry = new ProcessingCompletedLogEntry
        {
            SessionId = sessionId,
            EndTime = DateTime.UtcNow,
            TotalProcessingTimeMs = (int)summary.TotalProcessingTime.TotalMilliseconds,
            TotalBatches = summary.TotalBatches,
            SuccessfulBatches = summary.SuccessfulBatches,
            FailedBatches = summary.FailedBatches,
            TotalProductosNuevos = summary.TotalProductosNuevos,
            TotalProductosActualizados = summary.TotalProductosActualizados,
            TotalErrores = summary.TotalErrores,
            SuccessRate = summary.SuccessRate,
            Status = summary.IsSuccessful ? "COMPLETADO_EXITOSO" : "COMPLETADO_CON_ERRORES",
            ErrorDetails = summary.ErrorDetails,
            CategoriasNoEncontradas = summary.CategoriasNoEncontradas.ToList(),
            TotalCategoriasNoEncontradas = summary.CategoriasNoEncontradas.Count
        };

        await WriteLogEntryAsync(logEntry, "processing_completed");
        
        _logger.LogInformation("Log de finalización registrado para sesión {SessionId}: {Status}", 
            sessionId, logEntry.Status);
    }

    public async Task LogErrorAsync(string sessionId, string errorType, string errorMessage, Exception? exception = null)
    {
        var logEntry = new ErrorLogEntry
        {
            SessionId = sessionId,
            ErrorTime = DateTime.UtcNow,
            ErrorType = errorType,
            ErrorMessage = errorMessage,
            StackTrace = exception?.StackTrace,
            InnerException = exception?.InnerException?.Message
        };

        await WriteLogEntryAsync(logEntry, "error");
        
        _logger.LogError("Error registrado en log: {ErrorType} - {ErrorMessage}", errorType, errorMessage);
    }

    private async Task WriteLogEntryAsync<T>(T logEntry, string logType)
    {
        try
        {
            var fileName = $"{logType}_{DateTime.UtcNow:yyyyMMdd}.json";
            var filePath = Path.Combine(_logDirectory, fileName);
            
            var jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = false,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            var jsonLine = JsonSerializer.Serialize(logEntry, jsonOptions);
            await File.AppendAllTextAsync(filePath, jsonLine + Environment.NewLine);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error escribiendo log de tipo {LogType}", logType);
        }
    }

}

public class ProcessingCompletedLogEntry
{
    public string SessionId { get; set; } = string.Empty;
    public DateTime EndTime { get; set; }
    public int TotalProcessingTimeMs { get; set; }
    public int TotalBatches { get; set; }
    public int SuccessfulBatches { get; set; }
    public int FailedBatches { get; set; }
    public int TotalProductosNuevos { get; set; }
    public int TotalProductosActualizados { get; set; }
    public int TotalErrores { get; set; }
    public double SuccessRate { get; set; }
    public string Status { get; set; } = string.Empty;
    public List<string> ErrorDetails { get; set; } = new();
    public List<int> CategoriasNoEncontradas { get; set; } = new();
    public int TotalCategoriasNoEncontradas { get; set; }
}

public class ErrorLogEntry
{
    public string SessionId { get; set; } = string.Empty;
    public DateTime ErrorTime { get; set; }
    public string ErrorType { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public string? StackTrace { get; set; }
    public string? InnerException { get; set; }
}
