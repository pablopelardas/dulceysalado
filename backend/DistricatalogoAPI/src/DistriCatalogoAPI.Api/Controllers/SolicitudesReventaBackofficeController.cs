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
    [Route("api/backoffice/solicitudes-reventa")]
    [Authorize]
    public class SolicitudesReventaBackofficeController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SolicitudesReventaBackofficeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Listar todas las solicitudes (backoffice)
        /// </summary>
        [HttpGet]
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
        /// Aprobar o rechazar una solicitud (backoffice)
        /// </summary>
        [HttpPost("{id}/responder")]
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