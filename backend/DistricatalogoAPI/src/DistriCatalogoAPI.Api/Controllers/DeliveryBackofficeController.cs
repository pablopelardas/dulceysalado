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
    [Route("api/backoffice/delivery")]
    [Authorize] // Solo usuarios internos del backoffice
    public class DeliveryBackofficeController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<DeliveryBackofficeController> _logger;

        public DeliveryBackofficeController(IMediator mediator, ILogger<DeliveryBackofficeController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        /// <summary>
        /// Obtener configuración de delivery para una empresa (Backoffice)
        /// </summary>
        [HttpGet("settings/{empresaId}")]
        public async Task<ActionResult<DeliverySettingsDto>> GetDeliverySettings(int empresaId)
        {
            _logger.LogDebug("Getting delivery settings for empresa: {EmpresaId} from backoffice", empresaId);

            // Validar que el usuario tenga permisos para ver esta empresa
            var empresaIdClaim = User.FindFirst("empresaId")?.Value;
            if (string.IsNullOrEmpty(empresaIdClaim) || !int.TryParse(empresaIdClaim, out var requestingEmpresaId))
            {
                return Unauthorized(new { message = "Token inválido" });
            }

            var query = new GetDeliverySettingsQuery { EmpresaId = empresaId };
            var result = await _mediator.Send(query);

            if (result == null)
            {
                return NotFound($"No se encontraron configuraciones de delivery para la empresa {empresaId}");
            }

            return Ok(result);
        }

        /// <summary>
        /// Crear configuración de delivery para una empresa (Backoffice)
        /// </summary>
        [HttpPost("settings")]
        public async Task<ActionResult<DeliverySettingsDto>> CreateDeliverySettings([FromBody] CreateDeliverySettingsDto dto)
        {
            _logger.LogDebug("Creating delivery settings for empresa: {EmpresaId} from backoffice", dto.EmpresaId);

            // Validar permisos del usuario
            var empresaIdClaim = User.FindFirst("empresaId")?.Value;
            if (string.IsNullOrEmpty(empresaIdClaim) || !int.TryParse(empresaIdClaim, out var requestingEmpresaId))
            {
                return Unauthorized(new { message = "Token inválido" });
            }

            var command = new CreateDeliverySettingsCommand
            {
                EmpresaId = dto.EmpresaId,
                MinSlotsAhead = dto.MinSlotsAhead,
                MaxCapacityMorning = dto.MaxCapacityMorning,
                MaxCapacityAfternoon = dto.MaxCapacityAfternoon,
                
                // Lunes
                MondayEnabled = dto.MondayEnabled,
                MondayMorningStart = dto.MondayMorningStart,
                MondayMorningEnd = dto.MondayMorningEnd,
                MondayAfternoonStart = dto.MondayAfternoonStart,
                MondayAfternoonEnd = dto.MondayAfternoonEnd,
                
                // Martes
                TuesdayEnabled = dto.TuesdayEnabled,
                TuesdayMorningStart = dto.TuesdayMorningStart,
                TuesdayMorningEnd = dto.TuesdayMorningEnd,
                TuesdayAfternoonStart = dto.TuesdayAfternoonStart,
                TuesdayAfternoonEnd = dto.TuesdayAfternoonEnd,
                
                // Miércoles
                WednesdayEnabled = dto.WednesdayEnabled,
                WednesdayMorningStart = dto.WednesdayMorningStart,
                WednesdayMorningEnd = dto.WednesdayMorningEnd,
                WednesdayAfternoonStart = dto.WednesdayAfternoonStart,
                WednesdayAfternoonEnd = dto.WednesdayAfternoonEnd,
                
                // Jueves
                ThursdayEnabled = dto.ThursdayEnabled,
                ThursdayMorningStart = dto.ThursdayMorningStart,
                ThursdayMorningEnd = dto.ThursdayMorningEnd,
                ThursdayAfternoonStart = dto.ThursdayAfternoonStart,
                ThursdayAfternoonEnd = dto.ThursdayAfternoonEnd,
                
                // Viernes
                FridayEnabled = dto.FridayEnabled,
                FridayMorningStart = dto.FridayMorningStart,
                FridayMorningEnd = dto.FridayMorningEnd,
                FridayAfternoonStart = dto.FridayAfternoonStart,
                FridayAfternoonEnd = dto.FridayAfternoonEnd,
                
                // Sábado
                SaturdayEnabled = dto.SaturdayEnabled,
                SaturdayMorningStart = dto.SaturdayMorningStart,
                SaturdayMorningEnd = dto.SaturdayMorningEnd,
                SaturdayAfternoonStart = dto.SaturdayAfternoonStart,
                SaturdayAfternoonEnd = dto.SaturdayAfternoonEnd,
                
                // Domingo
                SundayEnabled = dto.SundayEnabled,
                SundayMorningStart = dto.SundayMorningStart,
                SundayMorningEnd = dto.SundayMorningEnd,
                SundayAfternoonStart = dto.SundayAfternoonStart,
                SundayAfternoonEnd = dto.SundayAfternoonEnd
            };

            try
            {
                var result = await _mediator.Send(command);
                return CreatedAtAction(nameof(GetDeliverySettings), new { empresaId = result.EmpresaId }, result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Actualizar configuración de delivery (Backoffice)
        /// </summary>
        [HttpPut("settings/{id}")]
        public async Task<ActionResult<DeliverySettingsDto>> UpdateDeliverySettings(int id, [FromBody] UpdateDeliverySettingsDto dto)
        {
            _logger.LogDebug("Updating delivery settings: {SettingsId} from backoffice", id);

            // Validar permisos del usuario
            var empresaIdClaim = User.FindFirst("empresaId")?.Value;
            if (string.IsNullOrEmpty(empresaIdClaim) || !int.TryParse(empresaIdClaim, out var requestingEmpresaId))
            {
                return Unauthorized(new { message = "Token inválido" });
            }

            var command = new UpdateDeliverySettingsCommand
            {
                Id = id,
                MinSlotsAhead = dto.MinSlotsAhead,
                MaxCapacityMorning = dto.MaxCapacityMorning,
                MaxCapacityAfternoon = dto.MaxCapacityAfternoon,
                
                // Lunes
                MondayEnabled = dto.MondayEnabled,
                MondayMorningStart = dto.MondayMorningStart,
                MondayMorningEnd = dto.MondayMorningEnd,
                MondayAfternoonStart = dto.MondayAfternoonStart,
                MondayAfternoonEnd = dto.MondayAfternoonEnd,
                
                // Martes
                TuesdayEnabled = dto.TuesdayEnabled,
                TuesdayMorningStart = dto.TuesdayMorningStart,
                TuesdayMorningEnd = dto.TuesdayMorningEnd,
                TuesdayAfternoonStart = dto.TuesdayAfternoonStart,
                TuesdayAfternoonEnd = dto.TuesdayAfternoonEnd,
                
                // Miércoles
                WednesdayEnabled = dto.WednesdayEnabled,
                WednesdayMorningStart = dto.WednesdayMorningStart,
                WednesdayMorningEnd = dto.WednesdayMorningEnd,
                WednesdayAfternoonStart = dto.WednesdayAfternoonStart,
                WednesdayAfternoonEnd = dto.WednesdayAfternoonEnd,
                
                // Jueves
                ThursdayEnabled = dto.ThursdayEnabled,
                ThursdayMorningStart = dto.ThursdayMorningStart,
                ThursdayMorningEnd = dto.ThursdayMorningEnd,
                ThursdayAfternoonStart = dto.ThursdayAfternoonStart,
                ThursdayAfternoonEnd = dto.ThursdayAfternoonEnd,
                
                // Viernes
                FridayEnabled = dto.FridayEnabled,
                FridayMorningStart = dto.FridayMorningStart,
                FridayMorningEnd = dto.FridayMorningEnd,
                FridayAfternoonStart = dto.FridayAfternoonStart,
                FridayAfternoonEnd = dto.FridayAfternoonEnd,
                
                // Sábado
                SaturdayEnabled = dto.SaturdayEnabled,
                SaturdayMorningStart = dto.SaturdayMorningStart,
                SaturdayMorningEnd = dto.SaturdayMorningEnd,
                SaturdayAfternoonStart = dto.SaturdayAfternoonStart,
                SaturdayAfternoonEnd = dto.SaturdayAfternoonEnd,
                
                // Domingo
                SundayEnabled = dto.SundayEnabled,
                SundayMorningStart = dto.SundayMorningStart,
                SundayMorningEnd = dto.SundayMorningEnd,
                SundayAfternoonStart = dto.SundayAfternoonStart,
                SundayAfternoonEnd = dto.SundayAfternoonEnd
            };

            try
            {
                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Obtener horarios específicos para una configuración de delivery (Backoffice)
        /// </summary>
        [HttpGet("schedules/{deliverySettingsId}")]
        public async Task<ActionResult<List<DeliveryScheduleDto>>> GetSchedules(int deliverySettingsId, [FromQuery] bool futureOnly = true)
        {
            _logger.LogDebug("Getting delivery schedules for settings: {DeliverySettingsId}, futureOnly: {FutureOnly} from backoffice", 
                deliverySettingsId, futureOnly);

            // Validar permisos del usuario
            var empresaIdClaim = User.FindFirst("empresaId")?.Value;
            if (string.IsNullOrEmpty(empresaIdClaim) || !int.TryParse(empresaIdClaim, out var requestingEmpresaId))
            {
                return Unauthorized(new { message = "Token inválido" });
            }

            var query = new GetDeliverySchedulesQuery 
            { 
                DeliverySettingsId = deliverySettingsId,
                FutureOnly = futureOnly
            };

            try
            {
                var result = await _mediator.Send(query);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Crear horario específico para una fecha (Backoffice)
        /// </summary>
        [HttpPost("schedules")]
        public async Task<ActionResult<DeliveryScheduleDto>> CreateSchedule([FromBody] CreateDeliveryScheduleRequest request)
        {
            _logger.LogDebug("Creating delivery schedule for settings: {DeliverySettingsId}, date: {Date} from backoffice", 
                request.DeliverySettingsId, request.Date);

            // Validar permisos del usuario
            var empresaIdClaim = User.FindFirst("empresaId")?.Value;
            if (string.IsNullOrEmpty(empresaIdClaim) || !int.TryParse(empresaIdClaim, out var requestingEmpresaId))
            {
                return Unauthorized(new { message = "Token inválido" });
            }

            var command = new CreateDeliveryScheduleCommand
            {
                DeliverySettingsId = request.DeliverySettingsId,
                Date = request.Date,
                MorningEnabled = request.MorningEnabled,
                AfternoonEnabled = request.AfternoonEnabled,
                CustomMaxCapacityMorning = request.CustomMaxCapacityMorning,
                CustomMaxCapacityAfternoon = request.CustomMaxCapacityAfternoon,
                CustomMorningStartTime = request.CustomMorningStartTime,
                CustomMorningEndTime = request.CustomMorningEndTime,
                CustomAfternoonStartTime = request.CustomAfternoonStartTime,
                CustomAfternoonEndTime = request.CustomAfternoonEndTime
            };

            try
            {
                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Actualizar horario específico para una fecha (Backoffice)
        /// </summary>
        [HttpPut("schedules/{id}")]
        public async Task<ActionResult<DeliveryScheduleDto>> UpdateSchedule(int id, [FromBody] UpdateDeliveryScheduleDto dto)
        {
            _logger.LogDebug("Updating delivery schedule {Id} from backoffice", id);

            // Validar permisos del usuario
            var empresaIdClaim = User.FindFirst("empresaId")?.Value;
            if (string.IsNullOrEmpty(empresaIdClaim) || !int.TryParse(empresaIdClaim, out var requestingEmpresaId))
            {
                return Unauthorized(new { message = "Token inválido" });
            }

            var command = new UpdateDeliveryScheduleCommand
            {
                Id = id,
                MorningEnabled = dto.MorningEnabled,
                AfternoonEnabled = dto.AfternoonEnabled,
                CustomMaxCapacityMorning = dto.CustomMaxCapacityMorning,
                CustomMaxCapacityAfternoon = dto.CustomMaxCapacityAfternoon,
                CustomMorningStartTime = dto.CustomMorningStartTime,
                CustomMorningEndTime = dto.CustomMorningEndTime,
                CustomAfternoonStartTime = dto.CustomAfternoonStartTime,
                CustomAfternoonEndTime = dto.CustomAfternoonEndTime
            };

            try
            {
                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Eliminar horario específico para una fecha (Backoffice)
        /// </summary>
        [HttpDelete("schedules/{id}")]
        public async Task<ActionResult> DeleteSchedule(int id)
        {
            _logger.LogDebug("Deleting delivery schedule {Id} from backoffice", id);

            // Validar permisos del usuario
            var empresaIdClaim = User.FindFirst("empresaId")?.Value;
            if (string.IsNullOrEmpty(empresaIdClaim) || !int.TryParse(empresaIdClaim, out var requestingEmpresaId))
            {
                return Unauthorized(new { message = "Token inválido" });
            }

            var command = new DeleteDeliveryScheduleCommand { Id = id };

            try
            {
                await _mediator.Send(command);
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Ver todos los slots con su ocupación (Backoffice - para administración)
        /// </summary>
        [HttpGet("slots/{empresaId}/admin")]
        public async Task<ActionResult<List<AvailableDeliverySlotDto>>> GetAdminSlots(
            int empresaId,
            [FromQuery] DateOnly? startDate = null,
            [FromQuery] DateOnly? endDate = null,
            [FromQuery] SlotType? slotType = null,
            [FromQuery] bool onlyAvailable = false) // Default false para ver todos
        {
            _logger.LogDebug("Getting admin delivery slots for empresa: {EmpresaId} from backoffice", empresaId);

            // Validar permisos del usuario
            var empresaIdClaim = User.FindFirst("empresaId")?.Value;
            if (string.IsNullOrEmpty(empresaIdClaim) || !int.TryParse(empresaIdClaim, out var requestingEmpresaId))
            {
                return Unauthorized(new { message = "Token inválido" });
            }

            var query = new GetAvailableDeliverySlotsQuery
            {
                EmpresaId = empresaId,
                StartDate = startDate,
                EndDate = endDate,
                SlotType = slotType,
                OnlyAvailable = onlyAvailable
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        private int? GetRequestingUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return userIdClaim != null ? int.Parse(userIdClaim) : null;
        }
    }

    // Request DTOs específicos para el controlador de backoffice
    public class CreateDeliveryScheduleRequest
    {
        public int DeliverySettingsId { get; set; }
        public DateOnly Date { get; set; }
        public bool MorningEnabled { get; set; } = true;
        public bool AfternoonEnabled { get; set; } = true;
        public int? CustomMaxCapacityMorning { get; set; }
        public int? CustomMaxCapacityAfternoon { get; set; }
        public TimeSpan? CustomMorningStartTime { get; set; }
        public TimeSpan? CustomMorningEndTime { get; set; }
        public TimeSpan? CustomAfternoonStartTime { get; set; }
        public TimeSpan? CustomAfternoonEndTime { get; set; }
    }
}