using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MediatR;
using DistriCatalogoAPI.Application.Commands.Users;
using DistriCatalogoAPI.Application.Queries.Users;
using DistriCatalogoAPI.Application.DTOs;
using System.Security.Claims;

namespace DistriCatalogoAPI.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Obtener lista de usuarios con paginación
        /// - Empresa Principal sin companyId: Ve TODOS los usuarios de todas las empresas
        /// - Empresa Principal con companyId: Ve usuarios de la empresa especificada
        /// - Empresa Cliente sin companyId: Ve solo usuarios de su propia empresa
        /// - Empresa Cliente con companyId: Error (no autorizado)
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<PagedResultDto<UserDto>>> GetUsers(
            [FromQuery] int? empresa_id = null,
            [FromQuery] bool includeInactive = false,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var requestingUserId = GetCurrentUserId();
                
                var query = new GetUsersListQuery
                {
                    EmpresaId = empresa_id,
                    IncludeInactive = includeInactive,
                    Page = page,
                    PageSize = pageSize,
                    RequestingUserId = requestingUserId
                };

                var result = await _mediator.Send(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Obtener un usuario por ID
        /// </summary>
        [HttpGet("{id}")]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            try
            {
                var requestingUserId = GetCurrentUserId();
                
                var query = new GetUserByIdQuery
                {
                    UserId = id,
                    RequestingUserId = requestingUserId
                };

                var result = await _mediator.Send(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Crear un nuevo usuario
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<UserDto>> CreateUser([FromBody] CreateUserDto createUserDto)
        {
            try
            {
                var requestingUserId = GetCurrentUserId();
                
                var command = new CreateUserCommand
                {
                    EmpresaId = createUserDto.EmpresaId,
                    Email = createUserDto.Email,
                    Password = createUserDto.Password,
                    Nombre = createUserDto.Nombre,
                    Apellido = createUserDto.Apellido,
                    Rol = createUserDto.Rol,
                    RequestingUserId = requestingUserId
                };

                var result = await _mediator.Send(command);
                return CreatedAtAction(nameof(GetUser), new { id = result.Id }, result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Actualizar un usuario
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ActionResult<UserDto>> UpdateUser(int id, [FromBody] UpdateUserDto updateUserDto)
        {
            try
            {
                var requestingUserId = GetCurrentUserId();
                
                var command = new UpdateUserCommand
                {
                    UserId = id,
                    Nombre = updateUserDto.Nombre,
                    Apellido = updateUserDto.Apellido,
                    Rol = updateUserDto.Rol,
                    PuedeGestionarProductosBase = updateUserDto.PuedeGestionarProductosBase,
                    PuedeGestionarProductosEmpresa = updateUserDto.PuedeGestionarProductosEmpresa,
                    PuedeGestionarCategoriasBase = updateUserDto.PuedeGestionarCategoriasBase,
                    PuedeGestionarCategoriasEmpresa = updateUserDto.PuedeGestionarCategoriasEmpresa,
                    PuedeGestionarUsuarios = updateUserDto.PuedeGestionarUsuarios,
                    PuedeVerEstadisticas = updateUserDto.PuedeVerEstadisticas,
                    RequestingUserId = requestingUserId
                };

                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Desactivar un usuario (soft delete)
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUser(int id)
        {
            try
            {
                var requestingUserId = GetCurrentUserId();
                
                var command = new DeleteUserCommand
                {
                    UserId = id,
                    RequestingUserId = requestingUserId
                };

                var result = await _mediator.Send(command);
                return Ok(new { message = "User deactivated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Cambiar contraseña de un usuario
        /// </summary>
        [HttpPut("{id}/password")]
        public async Task<ActionResult> ChangePassword(int id, [FromBody] ChangePasswordDto changePasswordDto)
        {
            try
            {
                // Clear model state to bypass automatic validation for CurrentPassword
                if (ModelState.ContainsKey("CurrentPassword"))
                {
                    ModelState.Remove("CurrentPassword");
                }

                var requestingUserId = GetCurrentUserId();
                
                var command = new ChangePasswordCommand
                {
                    UserId = id,
                    CurrentPassword = changePasswordDto.CurrentPassword,
                    NewPassword = changePasswordDto.NewPassword,
                    RequestingUserId = requestingUserId
                };

                var result = await _mediator.Send(command);
                return Ok(new { message = "Password changed successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("userId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out int userId) ? userId : null;
        }
    }
}