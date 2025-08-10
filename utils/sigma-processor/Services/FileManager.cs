using Microsoft.Extensions.Logging;
using SigmaProcessor.Config;

namespace SigmaProcessor.Services;

public class FileManager
{
    private readonly ProcessingConfig _config;
    private readonly ILogger<FileManager> _logger;

    public FileManager(ProcessingConfig config, ILogger<FileManager> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task<string> MoveToProcessedAsync(string originalFilePath)
    {
        try
        {
            EnsureDirectoryExists(_config.ProcessedPath);

            var fileName = Path.GetFileNameWithoutExtension(originalFilePath);
            var extension = Path.GetExtension(originalFilePath);
            var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            var newFileName = $"{fileName}_{timestamp}{extension}";
            var destinationPath = Path.Combine(_config.ProcessedPath, newFileName);

            await Task.Run(() => File.Move(originalFilePath, destinationPath));
            
            _logger.LogInformation("Archivo movido a procesados: {DestinationPath}", destinationPath);
            return destinationPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error moviendo archivo a procesados: {FilePath}", originalFilePath);
            throw;
        }
    }

    public void EnsureDirectoriesExist()
    {
        var directories = new[]
        {
            _config.InputPath,
            _config.ProcessedPath,
            _config.TempPath,
            _config.ErrorPath
        };

        foreach (var directory in directories)
        {
            EnsureDirectoryExists(directory);
        }

        _logger.LogInformation("Directorios verificados/creados: {Directories}", 
            string.Join(", ", directories));
    }

    private static void EnsureDirectoryExists(string path)
    {
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
    }

    public async Task CleanupTempFilesAsync()
    {
        try
        {
            if (!Directory.Exists(_config.TempPath))
                return;

            var tempFiles = Directory.GetFiles(_config.TempPath, "*", SearchOption.AllDirectories);
            
            foreach (var file in tempFiles)
            {
                try
                {
                    await Task.Run(() => File.Delete(file));
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("No se pudo eliminar archivo temporal {File}: {Error}", file, ex.Message);
                }
            }

            _logger.LogInformation("Limpieza de archivos temporales completada. Archivos eliminados: {Count}", tempFiles.Length);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error durante limpieza de archivos temporales");
        }
    }
}