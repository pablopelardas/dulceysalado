using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using DistriCatalogoAPI.Application.Commands.Delivery;
using DistriCatalogoAPI.Application.Queries.Delivery;
using DistriCatalogoAPI.Application.DTOs;
using DistriCatalogoAPI.Domain.Enums;
using System.Security.Claims;

namespace DistriCatalogoAPI.Api.Controllers
{
    [ApiController]
    [Route("api/cliente-auth/delivery")]
    [Authorize] // Clientes autenticados JWT
    public class DeliveryClienteController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<DeliveryClienteController> _logger;

        public DeliveryClienteController(IMediator mediator, ILogger<DeliveryClienteController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Obtener slots de delivery disponibles para el cliente
        /// </summary>
        [HttpGet("slots")]
        public async Task<ActionResult<List<AvailableDeliverySlotDto>>> GetAvailableSlots(
            [FromQuery] DateOnly? startDate = null,
            [FromQuery] DateOnly? endDate = null,
            [FromQuery] SlotType? slotType = null)
        {
            try
            {
                // Obtener empresa del token del cliente
                var empresaIdClaim = User.FindFirst("empresa_id")?.Value;
                if (string.IsNullOrEmpty(empresaIdClaim) || !int.TryParse(empresaIdClaim, out var empresaId))
                {
                    return Unauthorized(new { message = "Token de cliente inválido" });
                }

                _logger.LogDebug("Getting available delivery slots for empresa: {EmpresaId} from cliente", empresaId);

                var query = new GetAvailableDeliverySlotsQuery
                {
                    EmpresaId = empresaId,
                    StartDate = startDate,
                    EndDate = endDate,
                    SlotType = slotType,
                    OnlyAvailable = true // Los clientes solo ven slots disponibles
                };

                var result = await _mediator.Send(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting available delivery slots");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Reservar un slot de delivery (usado internamente cuando se crea un pedido)
        /// </summary>
        [HttpPost("reserve-slot")]
        public async Task<ActionResult<bool>> ReserveSlot([FromBody] ReserveDeliverySlotClienteRequest request)
        {
            try
            {
                // Obtener cliente y empresa del token
                var clienteIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var empresaIdClaim = User.FindFirst("empresa_id")?.Value;

                if (string.IsNullOrEmpty(clienteIdClaim) || string.IsNullOrEmpty(empresaIdClaim) ||
                    !int.TryParse(clienteIdClaim, out var clienteId) || !int.TryParse(empresaIdClaim, out var empresaId))
                {
                    return Unauthorized(new { message = "Token de cliente inválido" });
                }

                _logger.LogDebug("Reserving delivery slot for cliente: {ClienteId}, empresa: {EmpresaId}, date: {Date}, type: {SlotType}", 
                    clienteId, empresaId, request.Date, request.SlotType);

                var command = new ReserveDeliverySlotCommand
                {
                    EmpresaId = empresaId,
                    Date = request.Date,
                    SlotType = request.SlotType,
                    PedidoId = request.PedidoId
                };

                var result = await _mediator.Send(command);
                
                if (!result)
                {
                    return BadRequest(new { message = "No se pudo reservar el slot. Posiblemente esté lleno o no esté disponible." });
                }

                return Ok(new { success = true, message = "Slot reservado exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reserving delivery slot");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Verificar disponibilidad de un slot específico antes de crear el pedido
        /// </summary>
        [HttpPost("check-availability")]
        public async Task<ActionResult<SlotAvailabilityCheckDto>> CheckSlotAvailability([FromBody] CheckSlotAvailabilityRequest request)
        {
            try
            {
                // Obtener empresa del token del cliente
                var empresaIdClaim = User.FindFirst("empresa_id")?.Value;
                if (string.IsNullOrEmpty(empresaIdClaim) || !int.TryParse(empresaIdClaim, out var empresaId))
                {
                    return Unauthorized(new { message = "Token de cliente inválido" });
                }

                _logger.LogDebug("Checking slot availability for empresa: {EmpresaId}, date: {Date}, type: {SlotType}", 
                    empresaId, request.Date, request.SlotType);

                var query = new GetAvailableDeliverySlotsQuery
                {
                    EmpresaId = empresaId,
                    StartDate = request.Date,
                    EndDate = request.Date,
                    SlotType = request.SlotType,
                    OnlyAvailable = true
                };

                var result = await _mediator.Send(query);
                var daySlots = result.FirstOrDefault();
                
                if (daySlots == null)
                {
                    return Ok(new SlotAvailabilityCheckDto 
                    { 
                        IsAvailable = false, 
                        Message = "No hay slots disponibles para la fecha seleccionada" 
                    });
                }

                var slots = request.SlotType == SlotType.Morning ? daySlots.MorningSlots : daySlots.AfternoonSlots;
                var slotInfo = slots.FirstOrDefault();

                return Ok(new SlotAvailabilityCheckDto
                {
                    IsAvailable = slotInfo?.IsAvailable == true,
                    RemainingCapacity = slotInfo?.RemainingCapacity ?? 0,
                    MaxCapacity = slotInfo?.MaxCapacity ?? 0,
                    Message = slotInfo?.IsAvailable == true ? "Slot disponible" : "Slot no disponible"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking slot availability");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }
    }

    // Request DTOs específicos para el controlador de cliente
    public class ReserveDeliverySlotClienteRequest
    {
        public DateOnly Date { get; set; }
        public SlotType SlotType { get; set; }
        public int PedidoId { get; set; }
    }

    public class CheckSlotAvailabilityRequest
    {
        public DateOnly Date { get; set; }
        public SlotType SlotType { get; set; }
    }

    public class SlotAvailabilityCheckDto
    {
        public bool IsAvailable { get; set; }
        public int RemainingCapacity { get; set; }
        public int MaxCapacity { get; set; }
        public string Message { get; set; } = string.Empty;
    }
}