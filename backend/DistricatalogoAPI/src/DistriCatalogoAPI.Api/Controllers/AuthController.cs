using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using DistriCatalogoAPI.Application.Commands.Users;
using DistriCatalogoAPI.Application.Queries.Users;
using DistriCatalogoAPI.Application.DTOs;
using System.Security.Claims;

namespace DistriCatalogoAPI.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Iniciar sesión con email y contraseña
        /// </summary>
        /// <param name="loginDto">Datos de login</param>
        /// <returns>Token de acceso y datos del usuario</returns>
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginDto loginDto)
        {
            try
            {
                var command = new LoginCommand
                {
                    Email = loginDto.Email,
                    Password = loginDto.Password
                };

                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Renovar token de acceso usando refresh token
        /// </summary>
        /// <param name="refreshTokenDto">Refresh token</param>
        /// <returns>Nuevos tokens de acceso</returns>
        [HttpPost("refresh")]
        public async Task<ActionResult<AuthResponseDto>> RefreshToken([FromBody] RefreshTokenDto refreshTokenDto)
        {
            try
            {
                var command = new RefreshTokenCommand
                {
                    RefreshToken = refreshTokenDto.RefreshToken
                };

                var result = await _mediator.Send(command);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>
        /// Cerrar sesión (placeholder)
        /// </summary>
        [HttpPost("logout")]
        public async Task<ActionResult> Logout()
        {
            // TODO: Implementar logout (invalidar token)
            return Ok(new { message = "Logout successful" });
        }

        /// <summary>
        /// Obtener información del usuario autenticado
        /// </summary>
        /// <returns>Datos del usuario y empresa</returns>
        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<AuthResponseDto>> GetCurrentUser()
        {
            try
            {
                var userIdClaim = User.FindFirst("userId")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }

                var query = new GetCurrentUserQuery
                {
                    UserId = userId
                };

                var result = await _mediator.Send(query);
                return Ok(result);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}