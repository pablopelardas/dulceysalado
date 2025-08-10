using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;

namespace DistriCatalogoAPI.Api.Services;

public class FileService : IFileService
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<FileService> _logger;
    private readonly IConfiguration _configuration;
    private readonly string[] _allowedExtensions = { ".jpg", ".jpeg", ".png", ".webp" };
    private readonly string[] _allowedContentTypes = { "image/jpeg", "image/png", "image/webp" };
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5MB
    private readonly string _uploadsRootPath;

    public FileService(IWebHostEnvironment environment, ILogger<FileService> logger, IConfiguration configuration)
    {
        _environment = environment;
        _logger = logger;
        _configuration = configuration;
        _uploadsRootPath = _configuration["Application:UploadsPath"] ?? Path.Combine(_environment.ContentRootPath, "uploads");
    }

    public async Task<string> SaveProductImageAsync(IFormFile file, string productCode, string productDescription, string productType)
    {
        if (!IsValidImageFile(file))
            throw new InvalidOperationException("Archivo de imagen no válido");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = GenerateProductImageName(productCode, productDescription, extension);
        
        var uploadsDir = Path.Combine(_uploadsRootPath, "productos", productType.ToLower());
        Directory.CreateDirectory(uploadsDir);
        
        var filePath = Path.Combine(uploadsDir, fileName);
        
        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);
        
        _logger.LogInformation("Imagen guardada: {FilePath}", filePath);
        
        var baseUrl = _configuration["Application:BaseUrl"] ?? "";
        return $"{baseUrl}/api/images/{fileName}";
    }

    public void DeleteProductImage(string imagePath)
    {
        if (string.IsNullOrEmpty(imagePath))
            return;

        // Extraer el nombre del archivo de la URL completa
        var uri = new Uri(imagePath, UriKind.RelativeOrAbsolute);
        if (!uri.IsAbsoluteUri)
        {
            // Si es relativa, verificar que sea de nuestro sistema
            if (!imagePath.StartsWith("/api/images/"))
                return;
        }
        else
        {
            // Si es absoluta, verificar que sea de nuestro dominio
            var baseUrl = _configuration["Application:BaseUrl"] ?? "";
            if (!imagePath.StartsWith(baseUrl))
                return;
        }

        var fileName = Path.GetFileName(uri.LocalPath);
        var baseUploadsDir = Path.Combine(_uploadsRootPath, "productos");
        
        // Buscar en ambos directorios (base y empresa)
        var possiblePaths = new[]
        {
            Path.Combine(baseUploadsDir, "base", fileName),
            Path.Combine(baseUploadsDir, "empresa", fileName)
        };

        foreach (var filePath in possiblePaths)
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation("Imagen eliminada: {FilePath}", filePath);
                break;
            }
        }
    }

    public bool IsValidImageFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return false;

        if (file.Length > MaxFileSizeBytes)
            return false;

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!_allowedExtensions.Contains(extension))
            return false;

        if (!_allowedContentTypes.Contains(file.ContentType))
            return false;

        return true;
    }

    public string GenerateProductImageName(string productCode, string productDescription, string extension)
    {
        var sanitizedDescription = SanitizeFileName(productDescription);
        return $"{productCode}-{sanitizedDescription}{extension}";
    }

    public string SanitizeFileName(string input)
    {
        if (string.IsNullOrEmpty(input))
            return "producto";

        // Convertir a minúsculas y reemplazar espacios con guiones
        var sanitized = input.ToLowerInvariant().Trim();
        
        // Remover caracteres especiales, mantener solo letras, números y guiones
        sanitized = Regex.Replace(sanitized, @"[^a-z0-9\s-]", "");
        
        // Reemplazar espacios múltiples con uno solo
        sanitized = Regex.Replace(sanitized, @"\s+", " ");
        
        // Reemplazar espacios con guiones
        sanitized = sanitized.Replace(" ", "-");
        
        // Remover guiones múltiples
        sanitized = Regex.Replace(sanitized, @"-+", "-");
        
        // Remover guiones al inicio y final
        sanitized = sanitized.Trim('-');
        
        // Limitar longitud
        if (sanitized.Length > 50)
            sanitized = sanitized.Substring(0, 50).TrimEnd('-');
        
        return string.IsNullOrEmpty(sanitized) ? "producto" : sanitized;
    }

    public async Task<(byte[] content, string contentType)> GetImageAsync(string fileName)
    {
        var baseUploadsDir = Path.Combine(_uploadsRootPath, "productos");
        var companiesUploadsDir = Path.Combine(_uploadsRootPath, "empresas");
        
        // Buscar en directorios de productos (base y empresa)
        var possiblePaths = new[]
        {
            Path.Combine(baseUploadsDir, "base", fileName),
            Path.Combine(baseUploadsDir, "empresa", fileName)
        };

        // También buscar en directorios de empresas
        if (Directory.Exists(companiesUploadsDir))
        {
            var companyDirs = Directory.GetDirectories(companiesUploadsDir);
            var companyPaths = new List<string>();
            
            foreach (var companyDir in companyDirs)
            {
                companyPaths.Add(Path.Combine(companyDir, "logo", fileName));
                companyPaths.Add(Path.Combine(companyDir, "favicon", fileName));
            }
            
            possiblePaths = possiblePaths.Concat(companyPaths).ToArray();
        }

        foreach (var filePath in possiblePaths)
        {
            if (File.Exists(filePath))
            {
                var content = await File.ReadAllBytesAsync(filePath);
                var extension = Path.GetExtension(fileName).ToLowerInvariant();
                var contentType = extension switch
                {
                    ".jpg" or ".jpeg" => "image/jpeg",
                    ".png" => "image/png",
                    ".webp" => "image/webp",
                    _ => "application/octet-stream"
                };
                
                return (content, contentType);
            }
        }

        throw new FileNotFoundException($"Imagen no encontrada: {fileName}");
    }

    public async Task<string> SaveCompanyLogoAsync(IFormFile file, int companyId, string companyName)
    {
        if (!IsValidImageFile(file))
            throw new InvalidOperationException("Archivo de imagen no válido");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = GenerateCompanyImageName(companyId, companyName, "logo", extension);
        
        var uploadsDir = Path.Combine(_uploadsRootPath, "empresas", companyId.ToString(), "logo");
        Directory.CreateDirectory(uploadsDir);
        
        var filePath = Path.Combine(uploadsDir, fileName);
        
        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);
        
        _logger.LogInformation("Logo guardado para empresa {CompanyId}: {FilePath}", companyId, filePath);
        
        var baseUrl = _configuration["Application:BaseUrl"] ?? "";
        return $"{baseUrl}/api/images/{fileName}";
    }

    public async Task<string> SaveCompanyFaviconAsync(IFormFile file, int companyId, string companyName)
    {
        if (!IsValidImageFile(file))
            throw new InvalidOperationException("Archivo de imagen no válido");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var fileName = GenerateCompanyImageName(companyId, companyName, "favicon", extension);
        
        var uploadsDir = Path.Combine(_uploadsRootPath, "empresas", companyId.ToString(), "favicon");
        Directory.CreateDirectory(uploadsDir);
        
        var filePath = Path.Combine(uploadsDir, fileName);
        
        using var stream = new FileStream(filePath, FileMode.Create);
        await file.CopyToAsync(stream);
        
        _logger.LogInformation("Favicon guardado para empresa {CompanyId}: {FilePath}", companyId, filePath);
        
        var baseUrl = _configuration["Application:BaseUrl"] ?? "";
        return $"{baseUrl}/api/images/{fileName}";
    }

    public void DeleteCompanyLogo(int companyId)
    {
        var logoDir = Path.Combine(_uploadsRootPath, "empresas", companyId.ToString(), "logo");
        DeleteFilesInDirectory(logoDir);
        _logger.LogInformation("Logo eliminado para empresa {CompanyId}", companyId);
    }

    public void DeleteCompanyFavicon(int companyId)
    {
        var faviconDir = Path.Combine(_uploadsRootPath, "empresas", companyId.ToString(), "favicon");
        DeleteFilesInDirectory(faviconDir);
        _logger.LogInformation("Favicon eliminado para empresa {CompanyId}", companyId);
    }

    public string GenerateCompanyImageName(int companyId, string companyName, string imageType, string extension)
    {
        var sanitizedName = SanitizeFileName(companyName);
        return $"{companyId}-{sanitizedName}-{imageType}{extension}";
    }

    private void DeleteFilesInDirectory(string directory)
    {
        if (!Directory.Exists(directory))
            return;

        var files = Directory.GetFiles(directory);
        foreach (var file in files)
        {
            File.Delete(file);
            _logger.LogDebug("Archivo eliminado: {FilePath}", file);
        }
    }
}