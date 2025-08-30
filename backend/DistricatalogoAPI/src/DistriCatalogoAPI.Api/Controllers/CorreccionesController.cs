using Microsoft.AspNetCore.Mvc;
using MediatR;
using DistriCatalogoAPI.Application.Queries.Pedidos;
using DistriCatalogoAPI.Application.Commands.Pedidos;
using DistriCatalogoAPI.Application.DTOs;

namespace DistriCatalogoAPI.Api.Controllers
{
    [ApiController]
    [Route("api/public/correccion")]
    public class CorreccionesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CorreccionesController> _logger;

        public CorreccionesController(IMediator mediator, ILogger<CorreccionesController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Obtener detalles de corrección por token (público)
        /// </summary>
        [HttpGet("{token}")]
        public async Task<ActionResult<CorreccionDto>> GetCorreccion(string token)
        {
            try
            {
                var query = new GetCorreccionByTokenQuery { Token = token };
                var correccion = await _mediator.Send(query);

                if (correccion == null)
                {
                    return NotFound(new { message = "Token de corrección no válido o expirado" });
                }

                return Ok(correccion);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo corrección por token {Token}", token);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Aprobar corrección (público)
        /// </summary>
        [HttpPost("{token}/aprobar")]
        public async Task<ActionResult> AprobarCorreccion(string token, [FromBody] ResponderCorreccionDto dto)
        {
            try
            {
                var command = new ResponderCorreccionCommand 
                { 
                    Token = token, 
                    Aprobado = true,
                    ComentarioCliente = dto.Comentario
                };

                var resultado = await _mediator.Send(command);

                if (resultado)
                {
                    _logger.LogInformation("Corrección aprobada para token {Token}", token);
                    return Ok(new { message = "Corrección aprobada exitosamente", aprobado = true });
                }
                else
                {
                    return BadRequest(new { message = "No se pudo procesar la respuesta" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error aprobando corrección para token {Token}", token);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Rechazar corrección (público)
        /// </summary>
        [HttpPost("{token}/rechazar")]
        public async Task<ActionResult> RechazarCorreccion(string token, [FromBody] ResponderCorreccionDto dto)
        {
            try
            {
                var command = new ResponderCorreccionCommand 
                { 
                    Token = token, 
                    Aprobado = false,
                    ComentarioCliente = dto.Comentario
                };

                var resultado = await _mediator.Send(command);

                if (resultado)
                {
                    _logger.LogInformation("Corrección rechazada para token {Token}", token);
                    return Ok(new { message = "Corrección rechazada", aprobado = false });
                }
                else
                {
                    return BadRequest(new { message = "No se pudo procesar la respuesta" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rechazando corrección para token {Token}", token);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }
    }

    public class ResponderCorreccionDto
    {
        public string? Comentario { get; set; }
    }
}