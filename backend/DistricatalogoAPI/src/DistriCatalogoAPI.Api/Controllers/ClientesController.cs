using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using DistriCatalogoAPI.Application.Commands.Clientes;
using DistriCatalogoAPI.Application.Queries.Clientes;
using DistriCatalogoAPI.Application.DTOs;
using System.Security.Claims;

namespace DistriCatalogoAPI.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ClientesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ClientesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Obtener lista de clientes con paginación y filtros
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PagedResultDto<ClienteDto>>> GetClientes(
            [FromQuery] int empresa_id,
            [FromQuery] string? search = null,
            [FromQuery] int? lista_precio_id = null,
            [FromQuery] bool? is_active = null,
            [FromQuery] bool include_deleted = false,
            [FromQuery] int page = 1,
            [FromQuery] int page_size = 50)
        {
            try
            {
                var query = new GetClientesQuery
                {
                    EmpresaId = empresa_id,
                    SearchTerm = search,
                    ListaPrecioId = lista_precio_id,
                    IsActive = is_active,
                    IncludeDeleted = include_deleted,
                    Page = page,
                    PageSize = page_size
                };

                var result = await _mediator.Send(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al obtener clientes", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtener cliente por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<ClienteDto>> GetCliente(int id, [FromQuery] int empresa_id, [FromQuery] bool include_deleted = false)
        {
            try
            {
                var query = new GetClienteByIdQuery
                {
                    ClienteId = id,
                    EmpresaId = empresa_id,
                    IncludeDeleted = include_deleted
                };

                var result = await _mediator.Send(query);
                
                if (result == null)
                    return NotFound(new { message = "Cliente no encontrado" });

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al obtener cliente", error = ex.Message });
            }
        }

        /// <summary>
        /// Crear nuevo cliente
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ClienteDto>> CreateCliente([FromBody] CreateClienteCommand command)
        {
            try
            {
                // Obtener usuario actual para auditoría
                command.CreatedBy = GetCurrentUsername();
                
                var result = await _mediator.Send(command);
                return CreatedAtAction(nameof(GetCliente), new { id = result.Id, empresa_id = command.EmpresaId }, result);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al crear cliente", error = ex.Message });
            }
        }

        /// <summary>
        /// Actualizar cliente existente
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<ClienteDto>> UpdateCliente(int id, [FromBody] UpdateClienteCommand command)
        {
            try
            {
                command.Id = id;
                command.UpdatedBy = GetCurrentUsername();
                
                var result = await _mediator.Send(command);
                
                if (result == null)
                    return NotFound(new { message = "Cliente no encontrado" });

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al actualizar cliente", error = ex.Message });
            }
        }

        /// <summary>
        /// Eliminar cliente (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteCliente(int id, [FromQuery] int empresa_id)
        {
            try
            {
                var command = new DeleteClienteCommand
                {
                    Id = id,
                    EmpresaId = empresa_id,
                    UpdatedBy = GetCurrentUsername()
                };

                var result = await _mediator.Send(command);
                
                if (!result)
                    return NotFound(new { message = "Cliente no encontrado" });

                return Ok(new { message = "Cliente eliminado correctamente" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al eliminar cliente", error = ex.Message });
            }
        }

        /// <summary>
        /// Configurar credenciales de acceso para cliente
        /// </summary>
        [HttpPost("{id}/credentials")]
        public async Task<ActionResult> CreateClienteCredentials(int id, [FromBody] CreateClienteCredentialsCommand command)
        {
            try
            {
                command.ClienteId = id;
                command.CreatedBy = GetCurrentUsername();
                
                var result = await _mediator.Send(command);
                
                if (!result)
                    return NotFound(new { message = "Cliente no encontrado" });

                return Ok(new { message = "Credenciales configuradas correctamente" });
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al configurar credenciales", error = ex.Message });
            }
        }

        /// <summary>
        /// Actualizar contraseña de cliente (admin)
        /// </summary>
        [HttpPut("{id}/password")]
        public async Task<ActionResult> UpdateClientePassword(int id, [FromBody] UpdateClientePasswordCommand command)
        {
            try
            {
                command.ClienteId = id;
                command.UpdatedBy = GetCurrentUsername();
                
                var result = await _mediator.Send(command);
                
                if (!result)
                    return NotFound(new { message = "Cliente no encontrado" });

                return Ok(new { message = "Contraseña actualizada correctamente" });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al actualizar contraseña", error = ex.Message });
            }
        }

        private string? GetCurrentUsername()
        {
            return User.FindFirst(ClaimTypes.Name)?.Value;
        }
    }
}