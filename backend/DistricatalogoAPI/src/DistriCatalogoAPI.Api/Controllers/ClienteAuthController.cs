using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using DistriCatalogoAPI.Application.Queries.Clientes;
using DistriCatalogoAPI.Application.DTOs;
using DistriCatalogoAPI.Domain.Interfaces;
using System.Security.Claims;

namespace DistriCatalogoAPI.Api.Controllers
{
    [ApiController]
    [Route("api/cliente-auth")]
    public class ClienteAuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IClienteAuthService _clienteAuthService;
        private readonly IClienteRepository _clienteRepository;

        public ClienteAuthController(IMediator mediator, IClienteAuthService clienteAuthService, IClienteRepository clienteRepository)
        {
            _mediator = mediator;
            _clienteAuthService = clienteAuthService;
            _clienteRepository = clienteRepository;
        }

        /// <summary>
        /// Iniciar sesión de cliente con username y contraseña
        /// </summary>
        [HttpPost("login")]
        public async Task<ActionResult<ClienteAuthResponseDto>> Login([FromBody] ClienteLoginDto loginDto)
        {
            try
            {
                // Buscar cliente por username
                var query = new GetClienteByUsernameQuery
                {
                    EmpresaId = loginDto.EmpresaId,
                    Username = loginDto.Username,
                    IncludeDeleted = false
                };

                var cliente = await _mediator.Send(query);
                
                if (cliente == null)
                {
                    return Unauthorized(new { message = "Credenciales inválidas" });
                }

                // Buscar la entidad completa para autenticación
                var clienteEntity = await _clienteRepository.GetByIdAsync(cliente.Id);
                if (clienteEntity == null)
                {
                    return Unauthorized(new { message = "Credenciales inválidas" });
                }

                // Verificar contraseña usando hash almacenado
                var isValidPassword = await _clienteAuthService.VerifyPasswordAsync(loginDto.Password, clienteEntity.PasswordHash ?? "");
                
                if (!isValidPassword)
                {
                    return Unauthorized(new { message = "Credenciales inválidas" });
                }

                // Generar tokens
                var accessToken = await _clienteAuthService.GenerateAccessTokenAsync(clienteEntity);
                var refreshToken = await _clienteAuthService.GenerateRefreshTokenAsync(clienteEntity);

                // Activar cliente y actualizar último login
                if (!clienteEntity.IsActive)
                {
                    clienteEntity.Activate();
                    await _clienteRepository.UpdateAsync(clienteEntity);
                }
                await _clienteRepository.UpdateLastLoginAsync(cliente.Id);

                var response = new ClienteAuthResponseDto
                {
                    Message = "Login exitoso",
                    Cliente = cliente,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresIn = "24h"
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error en el proceso de autenticación", error = ex.Message });
            }
        }

        /// <summary>
        /// Renovar token de acceso usando refresh token
        /// </summary>
        [HttpPost("refresh")]
        public async Task<ActionResult<ClienteAuthResponseDto>> RefreshToken([FromBody] ClienteRefreshTokenDto refreshDto)
        {
            try
            {
                var newTokens = await _clienteAuthService.RefreshTokenAsync(refreshDto.RefreshToken);
                
                if (!newTokens.Success || string.IsNullOrEmpty(newTokens.NewAccessToken))
                {
                    return Unauthorized(new { message = "Refresh token inválido o expirado" });
                }

                var response = new ClienteAuthResponseDto
                {
                    Message = "Token renovado exitosamente",
                    AccessToken = newTokens.NewAccessToken,
                    RefreshToken = refreshDto.RefreshToken,
                    ExpiresIn = "24h"
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al renovar token", error = ex.Message });
            }
        }

        /// <summary>
        /// Cerrar sesión (invalidar refresh token)
        /// </summary>
        [HttpPost("logout")]
        [Authorize]
        public async Task<ActionResult> Logout([FromBody] ClienteRefreshTokenDto refreshDto)
        {
            try
            {
                await _clienteAuthService.RevokeRefreshTokenAsync(refreshDto.RefreshToken);
                return Ok(new { message = "Sesión cerrada exitosamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al cerrar sesión", error = ex.Message });
            }
        }

        /// <summary>
        /// Obtener información del cliente autenticado
        /// </summary>
        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<ClienteDto>> GetCurrentCliente()
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

                var query = new GetClienteByIdQuery
                {
                    ClienteId = clienteId,
                    EmpresaId = empresaId,
                    IncludeDeleted = false
                };

                var cliente = await _mediator.Send(query);
                
                if (cliente == null)
                {
                    return NotFound(new { message = "Cliente no encontrado" });
                }

                return Ok(cliente);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al obtener información del cliente", error = ex.Message });
            }
        }

        /// <summary>
        /// Cambiar contraseña del cliente autenticado
        /// </summary>
        [HttpPost("change-password")]
        [Authorize]
        public async Task<ActionResult> ChangePassword([FromBody] ClienteChangePasswordDto changePasswordDto)
        {
            try
            {
                var clienteIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(clienteIdClaim) || !int.TryParse(clienteIdClaim, out var clienteId))
                {
                    return Unauthorized(new { message = "Token inválido" });
                }

                // Cambiar contraseña
                var success = await _clienteAuthService.ChangePasswordAsync(clienteId, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);
                
                if (!success)
                {
                    return BadRequest(new { message = "Error al cambiar la contraseña" });
                }

                return Ok(new { message = "Contraseña cambiada exitosamente" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al cambiar contraseña", error = ex.Message });
            }
        }
    }

    // DTOs específicos para autenticación de clientes
    public class ClienteLoginDto
    {
        public int EmpresaId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class ClienteAuthResponseDto
    {
        public string Message { get; set; } = string.Empty;
        public ClienteDto? Cliente { get; set; }
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public string ExpiresIn { get; set; } = string.Empty;
    }

    public class ClienteChangePasswordDto
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}