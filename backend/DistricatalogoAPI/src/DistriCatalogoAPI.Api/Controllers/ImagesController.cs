using Microsoft.AspNetCore.Mvc;
using DistriCatalogoAPI.Api.Services;

namespace DistriCatalogoAPI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImagesController : ControllerBase
{
    private readonly IFileService _fileService;
    private readonly ILogger<ImagesController> _logger;

    public ImagesController(IFileService fileService, ILogger<ImagesController> logger)
    {
        _fileService = fileService;
        _logger = logger;
    }

    [HttpGet("{filename}")]
    [ResponseCache(Duration = 86400, Location = ResponseCacheLocation.Any)]
    public async Task<IActionResult> GetImage(string filename)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(filename))
                return BadRequest(new { message = "Nombre de archivo inválido" });

            var (content, contentType) = await _fileService.GetImageAsync(filename);
            
            Response.Headers.Append("Cache-Control", "public, max-age=86400");
            Response.Headers.Append("ETag", $"\"{filename.GetHashCode()}\"");
            
            return File(content, contentType);
        }
        catch (FileNotFoundException)
        {
            _logger.LogWarning("Imagen no encontrada: {FileName}", filename);
            return NotFound(new { message = "Imagen no encontrada" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener imagen: {FileName}", filename);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}