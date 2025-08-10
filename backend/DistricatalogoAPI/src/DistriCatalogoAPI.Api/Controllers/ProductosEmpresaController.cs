using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using DistriCatalogoAPI.Application.Queries.ProductosEmpresa;
using DistriCatalogoAPI.Application.Commands.ProductosEmpresa;
using DistriCatalogoAPI.Api.Services;

namespace DistriCatalogoAPI.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductosEmpresaController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ProductosEmpresaController> _logger;
    private readonly IFileService _fileService;

    public ProductosEmpresaController(IMediator mediator, ILogger<ProductosEmpresaController> logger, IFileService fileService)
    {
        _mediator = mediator;
        _logger = logger;
        _fileService = fileService;
    }

    [HttpGet]
    public async Task<ActionResult<GetProductosEmpresaByEmpresaQueryResult>> GetByEmpresa(
        [FromQuery] int? empresaId = null,
        [FromQuery] bool? visible = null,
        [FromQuery] bool? destacado = null,
        [FromQuery] int? codigoRubro = null,
        [FromQuery] string? busqueda = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = null,
        [FromQuery] string? sortOrder = null,
        [FromQuery] int? listaPrecioId = null,
        [FromQuery] bool? soloSinConfiguracion = null,
        [FromQuery] bool incluirSinExistencia = false)
    {
        try
        {
            var query = new GetProductosEmpresaByEmpresaQuery
            {
                EmpresaId = empresaId,
                Visible = visible,
                Destacado = destacado,
                CodigoRubro = codigoRubro,
                Busqueda = busqueda,
                Page = page,
                PageSize = pageSize,
                SortBy = sortBy,
                SortOrder = sortOrder,
                ListaPrecioId = listaPrecioId,
                SoloSinConfiguracion = soloSinConfiguracion,
                IncluirSinExistencia = incluirSinExistencia
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Acceso no autorizado al obtener productos empresa");
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener productos empresa");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GetProductoEmpresaByIdQueryResult>> GetById(int id)
    {
        try
        {
            var query = new GetProductoEmpresaByIdQuery { Id = id };
            var result = await _mediator.Send(query);
            
            if (result == null)
                return NotFound();
                
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Acceso no autorizado al obtener producto empresa por ID");
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener producto empresa por ID {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpPost]
    public async Task<ActionResult<CreateProductoEmpresaCommandResult>> Create([FromBody] CreateProductoEmpresaCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Acceso no autorizado al crear producto empresa");
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error de validación al crear producto empresa");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al crear producto empresa");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<UpdateProductoEmpresaCommandResult>> Update(int id, [FromBody] UpdateProductoEmpresaCommand command)
    {
        try
        {
            command.Id = id;
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Acceso no autorizado al actualizar producto empresa");
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error de validación al actualizar producto empresa");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar producto empresa");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        try
        {
            var command = new DeleteProductoEmpresaCommand { Id = id };
            var result = await _mediator.Send(command);
            
            if (result)
                return NoContent();
            else
                return NotFound();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Acceso no autorizado al eliminar producto empresa");
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error de validación al eliminar producto empresa");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar producto empresa");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpPost("{id}/upload-image")]
    public async Task<ActionResult> UploadImage(int id, IFormFile image)
    {
        try
        {
            // Validar archivo
            if (image == null || image.Length == 0)
                return BadRequest(new { message = "No se proporcionó imagen" });

            if (!_fileService.IsValidImageFile(image))
                return BadRequest(new { message = "Archivo inválido. Solo se permiten imágenes JPG, PNG o WEBP de máximo 5MB" });

            // Obtener producto
            var query = new GetProductoEmpresaByIdQuery { Id = id };
            var producto = await _mediator.Send(query);
            
            if (producto == null)
                return NotFound(new { message = "Producto no encontrado" });

            // Si el producto tiene imagen local, eliminarla
            if (!string.IsNullOrEmpty(producto.ImagenUrl) && 
                (producto.ImagenUrl.Contains("/api/images/") || producto.ImagenUrl.StartsWith("/api/images/")))
            {
                _fileService.DeleteProductImage(producto.ImagenUrl);
            }

            // Guardar nueva imagen
            var imagePath = await _fileService.SaveProductImageAsync(
                image, 
                producto.Codigo, 
                producto.Descripcion,
                "empresa"
            );

            // Actualizar producto con nueva URL
            var updateCommand = new UpdateProductoEmpresaCommand
            {
                Id = id,
                Descripcion = producto.Descripcion,
                CodigoRubro = producto.CodigoRubro,
                Existencia = producto.Existencia,
                ImagenUrl = imagePath,
                ImagenAlt = producto.ImagenAlt ?? producto.Descripcion,
                Visible = producto.Visible,
                Destacado = producto.Destacado,
                OrdenCategoria = producto.OrdenCategoria,
                DescripcionCorta = producto.DescripcionCorta,
                DescripcionLarga = producto.DescripcionLarga,
                Tags = producto.Tags,
                CodigoBarras = producto.CodigoBarras,
                Marca = producto.Marca,
                UnidadMedida = producto.UnidadMedida
            };

            await _mediator.Send(updateCommand);

            return Ok(new { imagenUrl = imagePath, message = "Imagen subida exitosamente" });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Acceso no autorizado al subir imagen de producto empresa");
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al subir imagen de producto empresa {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpDelete("{id}/image")]
    public async Task<ActionResult> DeleteImage(int id)
    {
        try
        {
            // Obtener producto
            var query = new GetProductoEmpresaByIdQuery { Id = id };
            var producto = await _mediator.Send(query);
            
            if (producto == null)
                return NotFound(new { message = "Producto no encontrado" });

            // Si el producto tiene imagen local, eliminarla
            if (!string.IsNullOrEmpty(producto.ImagenUrl) && 
                (producto.ImagenUrl.Contains("/api/images/") || producto.ImagenUrl.StartsWith("/api/images/")))
            {
                _fileService.DeleteProductImage(producto.ImagenUrl);

                // Actualizar producto sin imagen
                var updateCommand = new UpdateProductoEmpresaCommand
                {
                    Id = id,
                    Descripcion = producto.Descripcion,
                    CodigoRubro = producto.CodigoRubro,
                    Existencia = producto.Existencia,
                    ImagenUrl = null,
                    ImagenAlt = null,
                    Visible = producto.Visible,
                    Destacado = producto.Destacado,
                    OrdenCategoria = producto.OrdenCategoria,
                    DescripcionCorta = producto.DescripcionCorta,
                    DescripcionLarga = producto.DescripcionLarga,
                    Tags = producto.Tags,
                    CodigoBarras = producto.CodigoBarras,
                    Marca = producto.Marca,
                    UnidadMedida = producto.UnidadMedida
                };

                await _mediator.Send(updateCommand);
            }

            return NoContent();
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Acceso no autorizado al eliminar imagen de producto empresa");
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar imagen de producto empresa {Id}", id);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}