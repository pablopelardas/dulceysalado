using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SigmaProcessor.Config;
using SigmaProcessor.Models;
using SigmaProcessor.Services;
using SigmaProcessor.Services.Sigma;
using SigmaProcessor.Utils;

// Ensure the Microsoft.Extensions.Http package is installed in your project.
// You can install it via NuGet Package Manager or the following command:
// dotnet add package Microsoft.Extensions.Http

namespace SigmaProcessor;

class Program
{
    private static ILogger<Program>? _logger;
    private static AppSettings? _appSettings;
    private static ServiceProvider? _serviceProvider;

    static async Task<int> Main(string[] args)
    {
        var loggerFactory = Logger.CreateLoggerFactory();
        _logger = loggerFactory.CreateLogger<Program>();

        try
        {
            _logger.LogInformation("=== PROCESADOR CSV GECOM OPTIMIZADO - INICIANDO ===");
            
            // Configurar servicios
            await ConfigureServicesAsync();

            var apiService = _serviceProvider!.GetRequiredService<ApiService>();
            var fileManager = _serviceProvider.GetRequiredService<FileManager>();
            var fileLogger = _serviceProvider.GetRequiredService<FileLoggerService>();
            var retryService = _serviceProvider.GetRequiredService<RetryService>();

            // Verificar conexión API
            _logger.LogInformation("Verificando conexión con API...");
            var connectionSuccess = await retryService.ExecuteWithRetryAsync(
                () => apiService.AuthenticateAsync(),
                "Conexión inicial con API",
                result => result
            );

            if (!connectionSuccess)
            {
                _logger.LogError("No se pudo conectar a la API");
                return 1;
            }

            // Preparar directorios
            fileManager.EnsureDirectoriesExist();

            // Verificar si hay archivos SIGMA XML
            var sigmaXmlProcessor = _serviceProvider.GetRequiredService<SigmaXmlProcessor>();
            var sigmaValidator = _serviceProvider.GetRequiredService<SigmaValidator>();
            
            var sigmaFiles = CheckForSigmaFiles(sigmaXmlProcessor);
            
            (bool success, bool processingDone) result;

            if (sigmaFiles)
            {
                _logger.LogInformation("=== MODO SIGMA XML DETECTADO ===");
                result = await ProcessSigmaFilesAsync(sigmaXmlProcessor, sigmaValidator, apiService, 
                    fileManager, fileLogger, retryService);
            }
            else
            {
                _logger.LogError("No se encontraron archivos válidos para procesar");
                result = (false, false);
            }

            // Limpieza final
            await fileManager.CleanupTempFilesAsync();

            return result.success ? 0 : 1;
        }
        catch (Exception ex)
        {
            _logger?.LogError(ex, "Error crítico en el procesamiento");
            return 1;
        }
        finally
        {
            _serviceProvider?.Dispose();
            loggerFactory.Dispose();
        }
    }

    private static async Task ConfigureServicesAsync()
    {
        var configuration = ConfigurationHelper.BuildConfiguration();
        _appSettings = ConfigurationHelper.GetAppSettings(configuration);
        ConfigurationHelper.ValidateConfiguration(_appSettings);

        _logger!.LogInformation("Configuración cargada exitosamente");

        var services = new ServiceCollection();

        // Configuración
        services.AddSingleton(_appSettings.Api);
        services.AddSingleton(_appSettings.Processing);

        // Logging
        services.AddLogging(builder =>
        {
            builder.AddConsole().SetMinimumLevel(LogLevel.Information);
        });

        // HttpClient
        services.AddHttpClient<ApiService>();

        // Servicios
        services.AddTransient<ApiService>();
        services.AddTransient<RetryService>();
        services.AddTransient<FileManager>();
        services.AddTransient<FileLoggerService>();
        
        // Servicios SIGMA XML
        services.AddTransient<SigmaProductProcessor>();
        services.AddTransient<SigmaStockProcessor>();
        services.AddTransient<SigmaDataTransformer>();
        services.AddTransient<SigmaXmlProcessor>();
        services.AddTransient<SigmaValidator>();

        _serviceProvider = services.BuildServiceProvider();
    }

    private static List<List<ProductWithPricesAndStock>> CreateProductBatchesWithStock(List<ProductWithPricesAndStock> products, int batchSize)
    {
        var batches = new List<List<ProductWithPricesAndStock>>();
        
        for (int i = 0; i < products.Count; i += batchSize)
        {
            var batch = products.Skip(i).Take(batchSize).ToList();
            batches.Add(batch);
        }
        
        return batches;
    }

    private static async Task<BatchProcessingSummary> ProcessBatchesSequentiallyWithPricesAndStockAsync(
        List<List<ProductWithPricesAndStock>> batches,
        ApiService apiService,
        RetryService retryService,
        string sessionId)
    {
        var summary = new BatchProcessingSummary
        {
            TotalBatches = batches.Count,
            StartTime = DateTime.UtcNow
        };

        for (int i = 0; i < batches.Count; i++)
        {
            var batchNumber = i + 1;
            var batch = batches[i];

            try
            {
                // Convertir ProductWithPricesAndStock a ProductWithPrices (ahora con StocksPorEmpresa incluido)
                var productsWithPrices = batch.Select(p => new ProductWithPrices
                {
                    Codigo = p.Codigo,
                    Descripcion = p.Descripcion,
                    CodigoRubro = p.CodigoRubro,
                    CategoriaNombre = p.CategoriaNombre,
                    Existencia = p.Existencia,
                    Grupo1 = p.Grupo1,
                    Grupo2 = p.Grupo2,
                    Grupo3 = p.Grupo3,
                    FechaAlta = p.FechaAlta,
                    FechaModi = p.FechaModi,
                    Imputable = p.Imputable,
                    Disponible = p.Disponible,
                    CodigoUbicacion = p.CodigoUbicacion,
                    ListasPrecios = p.ListasPrecios,
                    Porcentaje = p.Porcentaje,
                    StocksPorEmpresa = p.StocksPorEmpresa // ← AHORA INCLUIMOS LAS EXISTENCIAS
                }).ToList();
                
                _logger?.LogInformation("✅ DTO enviado a API: {Count} productos con existencias por empresa incluidas", productsWithPrices.Count);

                var result = await retryService.ExecuteApiOperationWithRetryAsync(
                    () => apiService.ProcessBatchWithPricesAsync(productsWithPrices, batchNumber, sessionId),
                    $"Procesamiento del lote {batchNumber} con precios y existencias"
                );

                // Actualizar summary (similar a la lógica en BatchProcessor)
                summary.ProcessedBatches++;
                if (result.Success && result.Data != null)
                {
                    summary.SuccessfulBatches++;
                    summary.TotalProductosNuevos += result.Data.ProductosNuevos;
                    summary.TotalProductosActualizados += result.Data.ProductosActualizados;
                    summary.TotalErrores += result.Data.Errores;
                }
                else
                {
                    summary.FailedBatches++;
                    summary.TotalErrores += batch.Count;
                }
            }
            catch (Exception ex)
            {
                summary.FailedBatches++;
                summary.TotalErrores += batch.Count;
                _logger?.LogError(ex, "Error procesando lote {BatchNumber}", batchNumber);
            }
        }

        summary.EndTime = DateTime.UtcNow;
        summary.TotalProcessingTime = summary.EndTime - summary.StartTime;
        
        return summary;
    }

    private static bool CheckForSigmaFiles(SigmaXmlProcessor sigmaXmlProcessor)
    {
        var sigmaConfig = _appSettings?.Processing?.SigmaConfig;
        if (sigmaConfig?.Enabled != true)
        {
            _logger?.LogDebug("Configuración SIGMA deshabilitada");
            return false;
        }

        return sigmaXmlProcessor.DetectSigmaFiles(_appSettings.Processing.InputPath, sigmaConfig);
    }

    private static async Task<(bool success, bool processingDone)> ProcessSigmaFilesAsync(
        SigmaXmlProcessor sigmaXmlProcessor,
        SigmaValidator sigmaValidator,
        ApiService apiService,
        FileManager fileManager,
        FileLoggerService fileLogger,
        RetryService retryService)
    {
        var sigmaConfig = _appSettings!.Processing.SigmaConfig;
        string sessionId = string.Empty;

        try
        {
            // Validar configuración SIGMA
            var (configValid, configErrors) = sigmaValidator.ValidateConfiguration(sigmaConfig);
            if (!configValid)
            {
                foreach (var error in configErrors)
                {
                    _logger!.LogError("Error configuración SIGMA: {Error}", error);
                }
                return (false, true);
            }

            // Validar existencia de archivos
            var (filesValid, fileErrors) = sigmaValidator.ValidateFilesExist(_appSettings.Processing.InputPath, sigmaConfig);
            if (!filesValid)
            {
                foreach (var error in fileErrors)
                {
                    _logger!.LogError("Error archivos SIGMA: {Error}", error);
                }
                return (false, true);
            }

            // Iniciar sesión de sincronización
            _logger!.LogInformation("Iniciando sesión de sincronización SIGMA...");
            var sessionResponse = await apiService.StartSyncSessionAsync("SIGMA XML Processing");
            sessionId = sessionResponse?.Data?.SessionId ?? string.Empty;

            // Procesar archivos SIGMA
            var products = await sigmaXmlProcessor.ProcessSigmaFilesAsync(
                _appSettings.Processing.InputPath,
                _appSettings.Processing.TempPath,
                sigmaConfig);

            if (!products.Any())
            {
                _logger.LogWarning("No se procesaron productos SIGMA");
                await apiService.FinishSyncSessionAsync(sessionId);
                return (true, true);
            }

            // Procesar en lotes usando el flujo existente
            _logger.LogInformation("Enviando {Count} productos SIGMA a la API en lotes...", products.Count);
            
            var batches = CreateProductBatchesWithStock(products, _appSettings.Processing.BatchSize);
            var summary = await ProcessBatchesSequentiallyWithPricesAndStockAsync(
                batches, apiService, retryService, sessionId);

            // Cerrar sesión
            await apiService.FinishSyncSessionAsync(sessionId);

            // Log de resultados
            _logger.LogInformation("=== RESUMEN PROCESAMIENTO SIGMA ===");
            _logger.LogInformation("Productos procesados: {Total}", products.Count);
            _logger.LogInformation("Lotes exitosos: {Successful} de {Total}", summary.SuccessfulBatches, summary.TotalBatches);
            _logger.LogInformation("Productos actualizados: {Updated}", summary.TotalProductosActualizados);
            _logger.LogInformation("Errores: {Errors}", summary.TotalErrores);
            _logger.LogInformation("Tiempo total: {Time}", summary.TotalProcessingTime);

            // Mover archivos procesados si está configurado
            if (_appSettings.Processing.MoveProcessedFiles)
            {
                var productsPath = Path.Combine(_appSettings.Processing.InputPath, sigmaConfig.ProductsFileName);
                var stockPath = Path.Combine(_appSettings.Processing.InputPath, sigmaConfig.StockFileName);
                
                await fileManager.MoveToProcessedAsync(productsPath);
                await fileManager.MoveToProcessedAsync(stockPath);
                
                // Mover archivo de clientes si existe
                if (!string.IsNullOrWhiteSpace(sigmaConfig.ClientsFileName))
                {
                    var clientsPath = Path.Combine(_appSettings.Processing.InputPath, sigmaConfig.ClientsFileName);
                    if (File.Exists(clientsPath))
                    {
                        await fileManager.MoveToProcessedAsync(clientsPath);
                    }
                }
            }

            await fileLogger.LogProcessingCompletedAsync(sessionId, summary);

            return (summary.FailedBatches == 0, true);
        }
        catch (Exception ex)
        {
            _logger!.LogError(ex, "Error procesando archivos SIGMA");
            if (!string.IsNullOrEmpty(sessionId))
            {
                try
                {
                    await apiService.FinishSyncSessionAsync(sessionId);
                }
                catch (Exception finishEx)
                {
                    _logger.LogError(finishEx, "Error cerrando sesión SIGMA tras fallo");
                }
            }
            await fileLogger.LogErrorAsync(sessionId, "SIGMA_PROCESSING_ERROR", ex.Message, ex);
            return (false, true);
        }
    }

}
