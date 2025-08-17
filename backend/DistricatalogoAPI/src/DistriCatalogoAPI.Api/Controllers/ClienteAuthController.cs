using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using DistriCatalogoAPI.Application.Queries.Clientes;
using DistriCatalogoAPI.Application.Commands.Clientes;
using DistriCatalogoAPI.Application.DTOs;
using DistriCatalogoAPI.Domain.Interfaces;
using System.Security.Claims;
using Microsoft.Extensions.Caching.Memory;

namespace DistriCatalogoAPI.Api.Controllers
{
    [ApiController]
    [Route("api/cliente-auth")]
    public class ClienteAuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IClienteAuthService _clienteAuthService;
        private readonly IClienteRepository _clienteRepository;
        private readonly IGoogleAuthService _googleAuthService;
        private readonly IMemoryCache _cache;

        public ClienteAuthController(
            IMediator mediator, 
            IClienteAuthService clienteAuthService, 
            IClienteRepository clienteRepository,
            IGoogleAuthService googleAuthService,
            IMemoryCache cache)
        {
            _mediator = mediator;
            _clienteAuthService = clienteAuthService;
            _clienteRepository = clienteRepository;
            _googleAuthService = googleAuthService;
            _cache = cache;
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
        /// Registrar nuevo cliente
        /// </summary>
        [HttpPost("register")]
        public async Task<ActionResult<ClienteAuthResponseDto>> Register([FromBody] ClienteRegisterDto registerDto)
        {
            try
            {
                // Crear comando para registro
                var command = new RegisterClienteCommand
                {
                    EmpresaId = registerDto.EmpresaId,
                    Nombre = registerDto.Nombre,
                    Email = registerDto.Email,
                    Password = registerDto.Password,
                    Telefono = registerDto.Telefono,
                    Direccion = registerDto.Direccion
                };

                // Ejecutar registro
                var clienteDto = await _mediator.Send(command);

                // Buscar la entidad completa para generar tokens
                var clienteEntity = await _clienteRepository.GetByIdAsync(clienteDto.Id);
                if (clienteEntity == null)
                {
                    return BadRequest(new { message = "Error en el proceso de registro" });
                }

                // Generar tokens
                var accessToken = await _clienteAuthService.GenerateAccessTokenAsync(clienteEntity);
                var refreshToken = await _clienteAuthService.GenerateRefreshTokenAsync(clienteEntity);

                // Actualizar último login
                await _clienteRepository.UpdateLastLoginAsync(clienteDto.Id);

                var response = new ClienteAuthResponseDto
                {
                    Message = "Registro exitoso",
                    Cliente = clienteDto,
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    ExpiresIn = "24h"
                };

                return Ok(response);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error en el proceso de registro", error = ex.Message });
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

        /// <summary>
        /// Iniciar flujo de autenticación con Google
        /// </summary>
        [HttpGet("google")]
        public ActionResult InitiateGoogleAuth([FromQuery] string? redirect_uri = null, [FromQuery] int empresa_id = 1)
        {
            try
            {
                var state = Guid.NewGuid().ToString();
                
                var googleAuthData = new GoogleAuthState
                {
                    State = state,
                    RedirectUri = redirect_uri ?? "http://localhost:5173/auth/google/callback",
                    EmpresaId = empresa_id,
                    CreatedAt = DateTime.UtcNow
                };

                _cache.Set($"google_auth_{state}", googleAuthData, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10),
                    Size = 1
                });

                var authUrl = _googleAuthService.GenerateAuthorizationUrl(state);
                
                return Redirect(authUrl);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error iniciando autenticación con Google", error = ex.Message });
            }
        }

        /// <summary>
        /// Callback de Google OAuth
        /// </summary>
        [HttpGet("google/callback")]
        public async Task<ActionResult> GoogleCallback([FromQuery] string? code, [FromQuery] string? state, [FromQuery] string? error)
        {
            try
            {
                if (!string.IsNullOrEmpty(error))
                {
                    return Redirect($"http://localhost:5173/auth/google/callback?error={Uri.EscapeDataString(error)}");
                }

                if (string.IsNullOrEmpty(code) || string.IsNullOrEmpty(state))
                {
                    return Redirect("http://localhost:5173/auth/google/callback?error=missing_parameters");
                }

                if (!_cache.TryGetValue($"google_auth_{state}", out GoogleAuthState? authData) || authData == null)
                {
                    return Redirect("http://localhost:5173/auth/google/callback?error=invalid_state");
                }

                _cache.Remove($"google_auth_{state}");

                var tokenResponse = await _googleAuthService.ExchangeCodeForTokensAsync(code);
                if (tokenResponse == null)
                {
                    return Redirect("http://localhost:5173/auth/google/callback?error=token_exchange_failed");
                }

                var userInfo = await _googleAuthService.GetUserInfoAsync(tokenResponse.AccessToken);
                if (userInfo == null)
                {
                    return Redirect("http://localhost:5173/auth/google/callback?error=user_info_failed");
                }

                if (!userInfo.VerifiedEmail)
                {
                    return Redirect("http://localhost:5173/auth/google/callback?error=email_not_verified");
                }

                var (cliente, esNuevo) = await _googleAuthService.CreateOrUpdateClienteFromGoogleAsync(userInfo, authData.EmpresaId);
                if (cliente == null)
                {
                    return Redirect("http://localhost:5173/auth/google/callback?error=user_creation_failed");
                }

                var accessToken = await _clienteAuthService.GenerateAccessTokenAsync(cliente);
                var refreshToken = await _clienteAuthService.GenerateRefreshTokenAsync(cliente);

                var redirectUrl = $"{authData.RedirectUri}?token={Uri.EscapeDataString(accessToken)}&refresh_token={Uri.EscapeDataString(refreshToken)}&is_new={esNuevo.ToString().ToLower()}";
                
                return Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                return Redirect($"http://localhost:5173/auth/google/callback?error={Uri.EscapeDataString(ex.Message)}");
            }
        }

        /// <summary>
        /// Actualizar perfil del cliente autenticado
        /// </summary>
        [HttpPut("profile")]
        [Authorize]
        public async Task<ActionResult<ClienteDto>> UpdateProfile([FromBody] UpdateClienteProfileDto updateDto)
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

                // Buscar la entidad completa para actualización
                var clienteEntity = await _clienteRepository.GetByIdAsync(clienteId);
                if (clienteEntity == null)
                {
                    return NotFound(new { message = "Cliente no encontrado" });
                }

                // Verificar que el cliente pertenece a la empresa del token
                if (clienteEntity.EmpresaId != empresaId)
                {
                    return Forbid("No tiene permisos para actualizar este perfil");
                }

                // Actualizar solo los campos proporcionados (no nulos)
                if (!string.IsNullOrWhiteSpace(updateDto.Nombre))
                    clienteEntity.Nombre = updateDto.Nombre.Trim();
                
                if (!string.IsNullOrWhiteSpace(updateDto.Direccion))
                    clienteEntity.Direccion = updateDto.Direccion.Trim();
                
                if (!string.IsNullOrWhiteSpace(updateDto.Localidad))
                    clienteEntity.Localidad = updateDto.Localidad.Trim();
                
                if (!string.IsNullOrWhiteSpace(updateDto.Telefono))
                    clienteEntity.Telefono = updateDto.Telefono.Trim();
                
                if (!string.IsNullOrWhiteSpace(updateDto.Cuit))
                    clienteEntity.Cuit = updateDto.Cuit.Trim();
                
                if (!string.IsNullOrWhiteSpace(updateDto.Altura))
                    clienteEntity.Altura = updateDto.Altura.Trim();
                
                if (!string.IsNullOrWhiteSpace(updateDto.Provincia))
                    clienteEntity.Provincia = updateDto.Provincia.Trim();
                
                if (!string.IsNullOrWhiteSpace(updateDto.TipoIva))
                    clienteEntity.TipoIva = updateDto.TipoIva.Trim();

                clienteEntity.UpdatedAt = DateTime.UtcNow;

                await _clienteRepository.UpdateAsync(clienteEntity);

                // Retornar el cliente actualizado usando el query existente
                var query = new GetClienteByIdQuery
                {
                    ClienteId = clienteId,
                    EmpresaId = empresaId,
                    IncludeDeleted = false
                };

                var clienteActualizado = await _mediator.Send(query);
                
                return Ok(new { 
                    message = "Perfil actualizado exitosamente", 
                    cliente = clienteActualizado 
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = "Error al actualizar perfil", error = ex.Message });
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

    public class GoogleAuthState
    {
        public string State { get; set; } = string.Empty;
        public string RedirectUri { get; set; } = string.Empty;
        public int EmpresaId { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UpdateClienteProfileDto
    {
        public string? Nombre { get; set; }
        public string? Direccion { get; set; }
        public string? Localidad { get; set; }
        public string? Telefono { get; set; }
        public string? Cuit { get; set; }
        public string? Altura { get; set; }
        public string? Provincia { get; set; }
        public string? TipoIva { get; set; }
    }
}