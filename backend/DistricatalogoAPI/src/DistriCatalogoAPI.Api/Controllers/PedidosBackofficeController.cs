using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using DistriCatalogoAPI.Application.Commands.Pedidos;
using DistriCatalogoAPI.Application.Queries.Pedidos;
using DistriCatalogoAPI.Application.DTOs;
using DistriCatalogoAPI.Domain.Entities;
using System.Security.Claims;

namespace DistriCatalogoAPI.Api.Controllers
{
    [ApiController]
    [Route("api/backoffice/pedidos")]
    [Authorize] // Aquí puedes agregar roles específicos para el backoffice
    public class PedidosBackofficeController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PedidosBackofficeController> _logger;

        public PedidosBackofficeController(IMediator mediator, ILogger<PedidosBackofficeController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Obtener todos los pedidos con filtros (backoffice)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PedidosPagedResultDto>> ObtenerPedidos(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] PedidoEstado? estado = null,
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null,
            [FromQuery] int? clienteId = null,
            [FromQuery] string? numeroContiene = null)
        {
            try
            {
                var empresaIdClaim = User.FindFirst("empresa_id")?.Value;
                if (string.IsNullOrEmpty(empresaIdClaim) || !int.TryParse(empresaIdClaim, out var empresaId))
                {
                    return Unauthorized(new { message = "Token inválido" });
                }

                var query = new GetPedidosQuery
                {
                    EmpresaId = empresaId,
                    Page = page,
                    PageSize = Math.Min(pageSize, 100),
                    Estado = estado,
                    FechaDesde = fechaDesde,
                    FechaHasta = fechaHasta,
                    ClienteId = clienteId,
                    NumeroContiene = numeroContiene
                };

                var resultado = await _mediator.Send(query);
                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo pedidos en backoffice");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtener un pedido específico por ID (backoffice)
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<PedidoDto>> ObtenerPedido(int id)
        {
            try
            {
                var empresaIdClaim = User.FindFirst("empresa_id")?.Value;
                if (string.IsNullOrEmpty(empresaIdClaim) || !int.TryParse(empresaIdClaim, out var empresaId))
                {
                    return Unauthorized(new { message = "Token inválido" });
                }

                var query = new GetPedidoByIdQuery
                {
                    PedidoId = id,
                    EmpresaId = empresaId,
                    IncludeItems = true
                };

                var pedido = await _mediator.Send(query);

                if (pedido == null)
                {
                    return NotFound(new { message = "Pedido no encontrado" });
                }

                return Ok(pedido);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo pedido {PedidoId} en backoffice", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Aceptar un pedido
        /// </summary>
        [HttpPut("{id}/aceptar")]
        public async Task<ActionResult> AceptarPedido(int id)
        {
            try
            {
                var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(usuarioIdClaim) || !int.TryParse(usuarioIdClaim, out var usuarioId))
                {
                    return Unauthorized(new { message = "Token inválido" });
                }

                var command = new GestionarPedidoCommand
                {
                    PedidoId = id,
                    NuevoEstado = PedidoEstado.Aceptado,
                    UsuarioId = usuarioId,
                    UpdatedBy = usuarioId.ToString()
                };

                var resultado = await _mediator.Send(command);

                if (resultado)
                {
                    _logger.LogInformation("Pedido {PedidoId} aceptado por usuario {UsuarioId}", id, usuarioId);
                    return Ok(new { message = "Pedido aceptado exitosamente" });
                }
                else
                {
                    return BadRequest(new { message = "No se pudo aceptar el pedido" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error aceptando pedido {PedidoId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Rechazar un pedido
        /// </summary>
        [HttpPut("{id}/rechazar")]
        public async Task<ActionResult> RechazarPedido(int id, [FromBody] RechazarPedidoDto rechazarDto)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(rechazarDto.Motivo))
                {
                    return BadRequest(new { message = "El motivo de rechazo es requerido" });
                }

                var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(usuarioIdClaim) || !int.TryParse(usuarioIdClaim, out var usuarioId))
                {
                    return Unauthorized(new { message = "Token inválido" });
                }

                var command = new GestionarPedidoCommand
                {
                    PedidoId = id,
                    NuevoEstado = PedidoEstado.Rechazado,
                    UsuarioId = usuarioId,
                    Motivo = rechazarDto.Motivo,
                    UpdatedBy = usuarioId.ToString()
                };

                var resultado = await _mediator.Send(command);

                if (resultado)
                {
                    _logger.LogInformation("Pedido {PedidoId} rechazado por usuario {UsuarioId} con motivo: {Motivo}", 
                        id, usuarioId, rechazarDto.Motivo);
                    return Ok(new { message = "Pedido rechazado exitosamente" });
                }
                else
                {
                    return BadRequest(new { message = "No se pudo rechazar el pedido" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rechazando pedido {PedidoId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Completar un pedido
        /// </summary>
        [HttpPut("{id}/completar")]
        public async Task<ActionResult> CompletarPedido(int id)
        {
            try
            {
                var usuarioIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(usuarioIdClaim) || !int.TryParse(usuarioIdClaim, out var usuarioId))
                {
                    return Unauthorized(new { message = "Token inválido" });
                }

                var command = new GestionarPedidoCommand
                {
                    PedidoId = id,
                    NuevoEstado = PedidoEstado.Completado,
                    UsuarioId = usuarioId,
                    UpdatedBy = usuarioId.ToString()
                };

                var resultado = await _mediator.Send(command);

                if (resultado)
                {
                    _logger.LogInformation("Pedido {PedidoId} completado por usuario {UsuarioId}", id, usuarioId);
                    return Ok(new { message = "Pedido completado exitosamente" });
                }
                else
                {
                    return BadRequest(new { message = "No se pudo completar el pedido" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completando pedido {PedidoId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtener estadísticas de pedidos
        /// </summary>
        [HttpGet("estadisticas")]
        public async Task<ActionResult<PedidoEstadisticasDto>> ObtenerEstadisticas()
        {
            try
            {
                var empresaIdClaim = User.FindFirst("empresa_id")?.Value;
                if (string.IsNullOrEmpty(empresaIdClaim) || !int.TryParse(empresaIdClaim, out var empresaId))
                {
                    return Unauthorized(new { message = "Token inválido" });
                }

                var query = new GetPedidosEstadisticasQuery
                {
                    EmpresaId = empresaId
                };

                var estadisticas = await _mediator.Send(query);
                return Ok(estadisticas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo estadísticas de pedidos");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtener pedidos pendientes (para notificaciones)
        /// </summary>
        [HttpGet("pendientes")]
        public async Task<ActionResult<List<PedidoDto>>> ObtenerPedidosPendientes()
        {
            try
            {
                var empresaIdClaim = User.FindFirst("empresa_id")?.Value;
                if (string.IsNullOrEmpty(empresaIdClaim) || !int.TryParse(empresaIdClaim, out var empresaId))
                {
                    return Unauthorized(new { message = "Token inválido" });
                }

                var query = new GetPedidosQuery
                {
                    EmpresaId = empresaId,
                    Estado = PedidoEstado.Pendiente,
                    PageSize = 50
                };

                var resultado = await _mediator.Send(query);
                return Ok(resultado.Items);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo pedidos pendientes");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }
    }

    public class RechazarPedidoDto
    {
        public string Motivo { get; set; } = string.Empty;
    }
}