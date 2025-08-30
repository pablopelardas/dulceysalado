using DistriCatalogoAPI.Application.Commands.Users;
using DistriCatalogoAPI.Application.DTOs;
using DistriCatalogoAPI.Application.Interfaces;
using DistriCatalogoAPI.Application.Queries.Users;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DistriCatalogoAPI.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UserNotificationPreferencesController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ICurrentUserService _currentUserService;

        public UserNotificationPreferencesController(IMediator mediator, ICurrentUserService currentUserService)
        {
            _mediator = mediator;
            _currentUserService = currentUserService;
        }

        /// <summary>
        /// Obtener las preferencias de notificaciones del usuario actual
        /// </summary>
        [HttpGet]
        public async Task<ActionResult<UserNotificationPreferencesDto>> GetMyPreferences()
        {
            var userId = _currentUserService.GetCurrentUserId();

            var query = new GetUserNotificationPreferencesQuery
            {
                UserId = userId
            };

            var preferences = await _mediator.Send(query);

            // Si no existen preferencias, retornar valores por defecto
            if (preferences == null)
            {
                return Ok(new UserNotificationPreferencesDto
                {
                    UserId = userId,
                    NotificacionNuevosPedidos = true,
                    NotificacionCorreccionesAprobadas = true,
                    NotificacionCorreccionesRechazadas = true,
                    NotificacionPedidosCancelados = true
                });
            }

            return Ok(preferences);
        }

        /// <summary>
        /// Actualizar las preferencias de notificaciones del usuario actual
        /// </summary>
        [HttpPut]
        public async Task<ActionResult<UserNotificationPreferencesDto>> UpdateMyPreferences([FromBody] UpdateUserNotificationPreferencesCommand command)
        {
            var userId = _currentUserService.GetCurrentUserId();

            // Asegurar que el comando use el ID del usuario actual
            command.UserId = userId;

            var updatedPreferences = await _mediator.Send(command);
            return Ok(updatedPreferences);
        }

        /// <summary>
        /// Obtener las preferencias de notificaciones de un usuario específico
        /// </summary>
        [HttpGet("{userId}")]
        public async Task<ActionResult<UserNotificationPreferencesDto>> GetUserPreferences(int userId)
        {
            var query = new GetUserNotificationPreferencesQuery { UserId = userId };
            var preferences = await _mediator.Send(query);

            if (preferences == null)
                return NotFound();

            return Ok(preferences);
        }
    }
}