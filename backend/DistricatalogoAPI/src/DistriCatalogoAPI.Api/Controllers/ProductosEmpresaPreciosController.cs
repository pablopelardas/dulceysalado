using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using DistriCatalogoAPI.Application.Commands.ProductosEmpresaPrecios;
using DistriCatalogoAPI.Application.Queries.ProductosEmpresaPrecios;

namespace DistriCatalogoAPI.Api.Controllers;

[ApiController]
[Route("api/productos-empresa-precios")]
[Authorize]
public class ProductosEmpresaPreciosController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ProductosEmpresaPreciosController> _logger;

    public ProductosEmpresaPreciosController(IMediator mediator, ILogger<ProductosEmpresaPreciosController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/productos-empresa-precios/producto/{productoEmpresaId}
    /// Obtiene todos los precios de un producto empresa en todas las listas
    /// </summary>
    [HttpGet("producto/{productoEmpresaId}")]
    public async Task<ActionResult<GetPreciosPorProductoEmpresaQueryResult>> GetPreciosPorProductoEmpresa(int productoEmpresaId)
    {
        try
        {
            var query = new GetPreciosPorProductoEmpresaQuery { ProductoEmpresaId = productoEmpresaId };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Acceso no autorizado al obtener precios del producto empresa");
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener precios del producto empresa {ProductoId}", productoEmpresaId);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// GET /api/productos-empresa-precios/producto/{productoEmpresaId}/lista/{listaPrecioId}
    /// Obtiene el precio de un producto empresa en una lista específica
    /// </summary>
    [HttpGet("producto/{productoEmpresaId}/lista/{listaPrecioId}")]
    public async Task<ActionResult> GetPrecioPorProductoEmpresaYLista(int productoEmpresaId, int listaPrecioId)
    {
        try
        {
            // TODO: Crear query específica para productos empresa o usar directamente el repository
            _logger.LogInformation("Obteniendo precio para producto empresa {ProductoId} en lista {ListaId}", productoEmpresaId, listaPrecioId);
            return Ok(new { message = "Funcionalidad pendiente de implementar" });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Acceso no autorizado al obtener precio del producto empresa");
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener precio del producto empresa {ProductoId} en lista {ListaId}", productoEmpresaId, listaPrecioId);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// POST /api/productos-empresa-precios
    /// Crea o actualiza el precio de un producto empresa en una lista
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<UpsertProductoEmpresaPrecioCommandResult>> UpsertPrecioProductoEmpresa([FromBody] UpsertProductoEmpresaPrecioCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            
            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Acceso no autorizado al actualizar precio producto empresa");
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar precio producto empresa");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpPut("producto/{productoEmpresaId}/lista/{listaPrecioId}")]
    public async Task<ActionResult<UpsertProductoEmpresaPrecioCommandResult>> UpsertPrecio(
        int productoEmpresaId, 
        int listaPrecioId, 
        [FromBody] UpsertPrecioRequest request)
    {
        try
        {
            var command = new UpsertProductoEmpresaPrecioCommand
            {
                ProductoEmpresaId = productoEmpresaId,
                ListaPrecioId = listaPrecioId,
                Precio = request.Precio
            };

            var result = await _mediator.Send(command);
            
            if (result.Success)
                return Ok(result);
            else
                return BadRequest(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Acceso no autorizado al actualizar precio producto empresa");
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar precio producto empresa {ProductoId} lista {ListaId}", 
                productoEmpresaId, listaPrecioId);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    [HttpDelete("producto/{productoEmpresaId}/lista/{listaPrecioId}")]
    public async Task<ActionResult<DeleteProductoEmpresaPrecioCommandResult>> DeletePrecio(
        int productoEmpresaId, 
        int listaPrecioId)
    {
        try
        {
            var command = new DeleteProductoEmpresaPrecioCommand
            {
                ProductoEmpresaId = productoEmpresaId,
                ListaPrecioId = listaPrecioId
            };

            var result = await _mediator.Send(command);
            
            if (result.Success)
                return Ok(result);
            else
                return NotFound(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Acceso no autorizado al eliminar precio producto empresa");
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar precio producto empresa {ProductoId} lista {ListaId}", 
                productoEmpresaId, listaPrecioId);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }
}

public class UpsertPrecioRequest
{
    public decimal Precio { get; set; }
}