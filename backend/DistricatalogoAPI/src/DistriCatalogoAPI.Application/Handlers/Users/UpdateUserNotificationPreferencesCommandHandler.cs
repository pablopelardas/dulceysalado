using AutoMapper;
using DistriCatalogoAPI.Application.Commands.Users;
using DistriCatalogoAPI.Application.DTOs;
using DistriCatalogoAPI.Application.Interfaces;
using DistriCatalogoAPI.Domain.Entities;
using DistriCatalogoAPI.Domain.Interfaces;
using MediatR;

namespace DistriCatalogoAPI.Application.Handlers.Users
{
    public class UpdateUserNotificationPreferencesCommandHandler : IRequestHandler<UpdateUserNotificationPreferencesCommand, UserNotificationPreferencesDto>
    {
        private readonly IUserNotificationPreferencesRepository _repository;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;

        public UpdateUserNotificationPreferencesCommandHandler(
            IUserNotificationPreferencesRepository repository,
            ICurrentUserService currentUserService,
            IMapper mapper)
        {
            _repository = repository;
            _currentUserService = currentUserService;
            _mapper = mapper;
        }

        public async Task<UserNotificationPreferencesDto> Handle(UpdateUserNotificationPreferencesCommand request, CancellationToken cancellationToken)
        {
            // Buscar preferencias existentes
            var existingPreferences = await _repository.GetByUserIdAsync(request.UserId);

            if (existingPreferences == null)
            {
                // Crear nuevas preferencias si no existen
                var newPreferences = new UserNotificationPreferences
                {
                    UserId = request.UserId,
                    NotificacionNuevosPedidos = request.NotificacionNuevosPedidos,
                    NotificacionCorreccionesAprobadas = request.NotificacionCorreccionesAprobadas,
                    NotificacionCorreccionesRechazadas = request.NotificacionCorreccionesRechazadas,
                    NotificacionPedidosCancelados = request.NotificacionPedidosCancelados,
                    CreatedBy = _currentUserService.GetCurrentUserId().ToString(),
                    UpdatedBy = _currentUserService.GetCurrentUserId().ToString()
                };

                var createdPreferences = await _repository.CreateAsync(newPreferences);
                return _mapper.Map<UserNotificationPreferencesDto>(createdPreferences);
            }
            else
            {
                // Actualizar preferencias existentes
                existingPreferences.NotificacionNuevosPedidos = request.NotificacionNuevosPedidos;
                existingPreferences.NotificacionCorreccionesAprobadas = request.NotificacionCorreccionesAprobadas;
                existingPreferences.NotificacionCorreccionesRechazadas = request.NotificacionCorreccionesRechazadas;
                existingPreferences.NotificacionPedidosCancelados = request.NotificacionPedidosCancelados;
                existingPreferences.UpdatedBy = _currentUserService.GetCurrentUserId().ToString();

                await _repository.UpdateAsync(existingPreferences);
                return _mapper.Map<UserNotificationPreferencesDto>(existingPreferences);
            }
        }
    }
}