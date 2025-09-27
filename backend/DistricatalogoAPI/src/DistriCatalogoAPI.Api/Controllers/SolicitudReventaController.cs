using System.Threading.Tasks;
using DistriCatalogoAPI.Application.DTOs.SolicitudReventa;
using DistriCatalogoAPI.Application.Handlers.SolicitudReventa.Commands;
using DistriCatalogoAPI.Application.Handlers.SolicitudReventa.Queries;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DistriCatalogoAPI.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SolicitudReventaController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SolicitudReventaController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Crear una nueva solicitud de cuenta de reventa
        /// </summary>
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateSolicitud([FromBody] CreateSolicitudReventaDto dto)
        {
            var clienteIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var empresaIdClaim = User.FindFirst("empresa_id")?.Value;

            if (string.IsNullOrEmpty(clienteIdClaim) || string.IsNullOrEmpty(empresaIdClaim) ||
                !int.TryParse(clienteIdClaim, out var clienteId) || !int.TryParse(empresaIdClaim, out var empresaId))
            {
                return Unauthorized(new { message = "Token inválido" });
            }

            var command = new CreateSolicitudReventaCommand
            {
                ClienteId = clienteId,
                EmpresaId = empresaId,
                Datos = dto
            };

            try
            {
                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (System.InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Obtener la solicitud del cliente actual
        /// </summary>
        [HttpGet("mi-solicitud")]
        [Authorize]
        public async Task<IActionResult> GetMiSolicitud()
        {
            var clienteIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(clienteIdClaim) || !int.TryParse(clienteIdClaim, out var clienteId))
            {
                return Unauthorized(new { message = "Token inválido" });
            }

            var query = new GetSolicitudClienteQuery { ClienteId = clienteId };
            var result = await _mediator.Send(query);
            
            if (result == null)
            {
                return NotFound(new { message = "No hay solicitudes para este cliente" });
            }

            return Ok(result);
        }

        /// <summary>
        /// Listar todas las solicitudes (solo admin)
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "Admin,Usuario")]
        public async Task<IActionResult> GetSolicitudes(
            [FromQuery] string? estado = null,
            [FromQuery] string? search = null,
            [FromQuery] int page = 1,
            [FromQuery] int limit = 20,
            [FromQuery] string? sortBy = "fechaSolicitud",
            [FromQuery] string? sortOrder = "desc")
        {
            var empresaIdClaim = User.FindFirst("empresa_id")?.Value;

            if (string.IsNullOrEmpty(empresaIdClaim) || !int.TryParse(empresaIdClaim, out var empresaId))
            {
                empresaId = 1; // Default fallback
            }

            var query = new GetSolicitudesReventaQuery
            {
                EmpresaId = empresaId,
                Estado = estado,
                Search = search,
                Page = page,
                Limit = limit,
                SortBy = sortBy,
                SortOrder = sortOrder
            };

            var result = await _mediator.Send(query);
            return Ok(result);
        }

        /// <summary>
        /// Aprobar o rechazar una solicitud (solo admin)
        /// </summary>
        [HttpPost("{id}/responder")]
        [Authorize(Roles = "Admin,Usuario")]
        public async Task<IActionResult> ResponderSolicitud(int id, [FromBody] ResponderSolicitudDto dto)
        {
            var username = User.Identity?.Name ?? "Sistema";

            var command = new ResponderSolicitudReventaCommand
            {
                SolicitudId = id,
                RespondidoPor = username,
                Respuesta = dto
            };

            try
            {
                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (System.InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}