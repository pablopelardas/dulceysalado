using Microsoft.Extensions.Logging;
using SigmaProcessor.Config;

namespace SigmaProcessor.Services;

public class RetryService
{
    private readonly ProcessingConfig _processingConfig;
    private readonly ILogger<RetryService> _logger;

    public RetryService(ProcessingConfig processingConfig, ILogger<RetryService> logger)
    {
        _processingConfig = processingConfig;
        _logger = logger;
    }

    public async Task<T> ExecuteWithRetryAsync<T>(
        Func<Task<T>> operation,
        string operationName,
        Func<T, bool> isSuccess)
    {
        var attempt = 1;
        var maxRetries = _processingConfig.MaxRetries;
        var retryDelay = TimeSpan.FromSeconds(_processingConfig.RetryDelaySeconds);

        while (attempt <= maxRetries)
        {
            try
            {
                _logger.LogDebug("Ejecutando {OperationName} - Intento {Attempt}/{MaxRetries}", 
                    operationName, attempt, maxRetries);

                var result = await operation();

                if (isSuccess(result))
                {
                    if (attempt > 1)
                    {
                        _logger.LogInformation("{OperationName} exitosa en intento {Attempt}", 
                            operationName, attempt);
                    }
                    return result;
                }

                _logger.LogWarning("{OperationName} falló en intento {Attempt}: resultado no exitoso", 
                    operationName, attempt);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "{OperationName} falló en intento {Attempt}: {Error}", 
                    operationName, attempt, ex.Message);

                if (attempt == maxRetries)
                {
                    _logger.LogError("Se agotaron los reintentos para {OperationName} después de {MaxRetries} intentos", 
                        operationName, maxRetries);
                    throw;
                }
            }

            attempt++;
            
            if (attempt <= maxRetries)
            {
                _logger.LogInformation("Esperando {DelaySeconds} segundos antes del siguiente intento...", 
                    retryDelay.TotalSeconds);
                await Task.Delay(retryDelay);
            }
        }

        throw new InvalidOperationException($"Se agotaron los reintentos para {operationName}");
    }

    public async Task<ApiResponse<T>> ExecuteApiOperationWithRetryAsync<T>(
        Func<Task<ApiResponse<T>>> apiOperation,
        string operationName)
    {
        return await ExecuteWithRetryAsync(
            apiOperation,
            operationName,
            response => response.Success
        );
    }
}