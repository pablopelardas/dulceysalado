using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using DistriCatalogoAPI.Application.Commands.ProductosPrecios;
using DistriCatalogoAPI.Application.Queries.ProductosPrecios;

namespace DistriCatalogoAPI.Api.Controllers;

[ApiController]
[Route("api/productos-precios")]
[Authorize]
public class ProductosPreciosController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ProductosPreciosController> _logger;

    public ProductosPreciosController(IMediator mediator, ILogger<ProductosPreciosController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// GET /api/productos-precios/producto/{productoId}
    /// Obtiene todos los precios de un producto en todas las listas
    /// </summary>
    [HttpGet("producto/{productoId}")]
    public async Task<ActionResult<GetPreciosPorProductoQueryResult>> GetPreciosPorProducto(int productoId)
    {
        try
        {
            var query = new GetPreciosPorProductoQuery { ProductoId = productoId };
            var result = await _mediator.Send(query);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Acceso no autorizado al obtener precios del producto");
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener precios del producto {ProductoId}", productoId);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// GET /api/productos-precios/producto/{productoId}/lista/{listaPrecioId}
    /// Obtiene el precio de un producto en una lista específica
    /// </summary>
    [HttpGet("producto/{productoId}/lista/{listaPrecioId}")]
    public async Task<ActionResult<GetPrecioPorProductoYListaQueryResult>> GetPrecioPorProductoYLista(int productoId, int listaPrecioId)
    {
        try
        {
            var query = new GetPrecioPorProductoYListaQuery 
            { 
                ProductoId = productoId, 
                ListaPrecioId = listaPrecioId 
            };
            var result = await _mediator.Send(query);
            
            if (result == null)
                return NotFound(new { message = "Precio no encontrado para este producto y lista" });
                
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Acceso no autorizado al obtener precio del producto");
            return Forbid();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al obtener precio del producto {ProductoId} en lista {ListaId}", productoId, listaPrecioId);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// POST /api/productos-precios
    /// Crea o actualiza el precio de un producto en una lista
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<UpsertPrecioProductoCommandResult>> UpsertPrecioProducto([FromBody] UpsertPrecioProductoCommand command)
    {
        try
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Acceso no autorizado al actualizar precio");
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error de validación al actualizar precio");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar precio");
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// PUT /api/productos-precios/producto/{productoId}/lista/{listaPrecioId}
    /// Actualiza el precio de un producto en una lista específica
    /// </summary>
    [HttpPut("producto/{productoId}/lista/{listaPrecioId}")]
    public async Task<ActionResult<UpdatePrecioProductoCommandResult>> UpdatePrecioProducto(
        int productoId, 
        int listaPrecioId, 
        [FromBody] UpdatePrecioProductoRequest request)
    {
        try
        {
            var command = new UpdatePrecioProductoCommand
            {
                ProductoId = productoId,
                ListaPrecioId = listaPrecioId,
                Precio = request.Precio
            };
            
            var result = await _mediator.Send(command);
            return Ok(result);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Acceso no autorizado al actualizar precio");
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error de validación al actualizar precio");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al actualizar precio del producto {ProductoId} en lista {ListaId}", productoId, listaPrecioId);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    /// <summary>
    /// DELETE /api/productos-precios/producto/{productoId}/lista/{listaPrecioId}
    /// Elimina el precio de un producto en una lista específica
    /// </summary>
    [HttpDelete("producto/{productoId}/lista/{listaPrecioId}")]
    public async Task<ActionResult> DeletePrecioProducto(int productoId, int listaPrecioId)
    {
        try
        {
            var command = new DeletePrecioProductoCommand
            {
                ProductoId = productoId,
                ListaPrecioId = listaPrecioId
            };
            
            var result = await _mediator.Send(command);
            
            if (result)
                return NoContent();
            else
                return NotFound(new { message = "Precio no encontrado para eliminar" });
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Acceso no autorizado al eliminar precio");
            return Forbid();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Error de validación al eliminar precio");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al eliminar precio del producto {ProductoId} en lista {ListaId}", productoId, listaPrecioId);
            return StatusCode(500, new { message = "Error interno del servidor" });
        }
    }

    public class UpdatePrecioProductoRequest
    {
        public decimal Precio { get; set; }
    }
}