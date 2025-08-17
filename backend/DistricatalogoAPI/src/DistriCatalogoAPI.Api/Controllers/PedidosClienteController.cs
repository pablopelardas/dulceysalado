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
    [Route("api/cliente-auth/pedidos")]
    [Authorize]
    public class PedidosClienteController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<PedidosClienteController> _logger;

        public PedidosClienteController(IMediator mediator, ILogger<PedidosClienteController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Crear un nuevo pedido
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<PedidoDto>> CrearPedido([FromBody] CrearPedidoDto crearPedidoDto)
        {
            try
            {
                var clienteIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var empresaIdClaim = User.FindFirst("empresa_id")?.Value;

                if (string.IsNullOrEmpty(clienteIdClaim) || string.IsNullOrEmpty(empresaIdClaim) ||
                    !int.TryParse(clienteIdClaim, out var clienteId) || !int.TryParse(empresaIdClaim, out var empresaId))
                {
                    return Unauthorized(new { message = "Token inválido" });
                }

                var command = new CrearPedidoCommand
                {
                    ClienteId = clienteId,
                    EmpresaId = empresaId,
                    Items = crearPedidoDto.Items,
                    Observaciones = crearPedidoDto.Observaciones,
                    DireccionEntrega = crearPedidoDto.DireccionEntrega,
                    FechaEntrega = crearPedidoDto.FechaEntrega,
                    HorarioEntrega = crearPedidoDto.HorarioEntrega,
                    CreatedBy = clienteId.ToString()
                };

                var pedido = await _mediator.Send(command);

                _logger.LogInformation("Pedido {PedidoId} creado por cliente {ClienteId}", pedido.Id, clienteId);

                return CreatedAtAction(nameof(ObtenerPedido), new { id = pedido.Id }, pedido);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creando pedido");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtener historial de pedidos del cliente autenticado
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PedidosPagedResultDto>> ObtenerHistorialPedidos(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] PedidoEstado? estado = null,
            [FromQuery] DateTime? fechaDesde = null,
            [FromQuery] DateTime? fechaHasta = null)
        {
            try
            {
                var clienteIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var empresaIdClaim = User.FindFirst("empresa_id")?.Value;

                if (string.IsNullOrEmpty(clienteIdClaim) || string.IsNullOrEmpty(empresaIdClaim) ||
                    !int.TryParse(clienteIdClaim, out var clienteId) || !int.TryParse(empresaIdClaim, out var empresaId))
                {
                    return Unauthorized(new { message = "Token inválido" });
                }

                var query = new GetPedidosByClienteQuery
                {
                    ClienteId = clienteId,
                    EmpresaId = empresaId,
                    Page = page,
                    PageSize = Math.Min(pageSize, 100), // Límite máximo
                    Estado = estado,
                    FechaDesde = fechaDesde,
                    FechaHasta = fechaHasta
                };

                var resultado = await _mediator.Send(query);

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo historial de pedidos");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtener un pedido específico por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<PedidoDto>> ObtenerPedido(int id)
        {
            try
            {
                var clienteIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var empresaIdClaim = User.FindFirst("empresa_id")?.Value;

                if (string.IsNullOrEmpty(clienteIdClaim) || string.IsNullOrEmpty(empresaIdClaim) ||
                    !int.TryParse(clienteIdClaim, out var clienteId) || !int.TryParse(empresaIdClaim, out var empresaId))
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

                // Verificar que el pedido pertenece al cliente autenticado
                if (pedido.ClienteId != clienteId)
                {
                    return Forbid("No tiene permisos para ver este pedido");
                }

                return Ok(pedido);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo pedido {PedidoId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Cancelar un pedido (solo si está pendiente)
        /// </summary>
        [HttpPut("{id}/cancelar")]
        public async Task<ActionResult> CancelarPedido(int id, [FromBody] CancelarPedidoDto cancelarDto)
        {
            try
            {
                var clienteIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var empresaIdClaim = User.FindFirst("empresa_id")?.Value;

                if (string.IsNullOrEmpty(clienteIdClaim) || string.IsNullOrEmpty(empresaIdClaim) ||
                    !int.TryParse(clienteIdClaim, out var clienteId) || !int.TryParse(empresaIdClaim, out var empresaId))
                {
                    return Unauthorized(new { message = "Token inválido" });
                }

                // Verificar que el pedido pertenece al cliente
                var pedidoQuery = new GetPedidoByIdQuery
                {
                    PedidoId = id,
                    EmpresaId = empresaId
                };

                var pedido = await _mediator.Send(pedidoQuery);
                if (pedido == null || pedido.ClienteId != clienteId)
                {
                    return NotFound(new { message = "Pedido no encontrado" });
                }

                if (pedido.Estado != "Pendiente")
                {
                    return BadRequest(new { message = "Solo se pueden cancelar pedidos pendientes" });
                }

                var command = new GestionarPedidoCommand
                {
                    PedidoId = id,
                    NuevoEstado = PedidoEstado.Cancelado,
                    UsuarioId = clienteId,
                    Motivo = cancelarDto.Motivo ?? "Cancelado por el cliente",
                    UpdatedBy = clienteId.ToString()
                };

                var resultado = await _mediator.Send(command);

                if (resultado)
                {
                    _logger.LogInformation("Pedido {PedidoId} cancelado por cliente {ClienteId}", id, clienteId);
                    return Ok(new { message = "Pedido cancelado exitosamente" });
                }
                else
                {
                    return BadRequest(new { message = "No se pudo cancelar el pedido" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelando pedido {PedidoId}", id);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }
    }

    public class CancelarPedidoDto
    {
        public string? Motivo { get; set; }
    }
}