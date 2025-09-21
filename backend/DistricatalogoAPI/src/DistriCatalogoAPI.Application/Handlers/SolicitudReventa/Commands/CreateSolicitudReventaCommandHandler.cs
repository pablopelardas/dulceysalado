using System;
using System.Threading;
using System.Threading.Tasks;
using DistriCatalogoAPI.Application.DTOs.SolicitudReventa;
using DistriCatalogoAPI.Domain.Entities;
using DistriCatalogoAPI.Domain.Interfaces;
using DistriCatalogoAPI.Application.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;

namespace DistriCatalogoAPI.Application.Handlers.SolicitudReventa.Commands
{
    public class CreateSolicitudReventaCommandHandler : IRequestHandler<CreateSolicitudReventaCommand, SolicitudReventaDto>
    {
        private readonly ISolicitudReventaRepository _repository;
        private readonly IClienteRepository _clienteRepository;
        private readonly ILogger<CreateSolicitudReventaCommandHandler> _logger;
        private readonly INotificationService _notificationService;

        public CreateSolicitudReventaCommandHandler(
            ISolicitudReventaRepository repository,
            IClienteRepository clienteRepository,
            ILogger<CreateSolicitudReventaCommandHandler> logger,
            INotificationService notificationService)
        {
            _repository = repository;
            _clienteRepository = clienteRepository;
            _logger = logger;
            _notificationService = notificationService;
        }

        public async Task<SolicitudReventaDto> Handle(CreateSolicitudReventaCommand request, CancellationToken cancellationToken)
        {
            // Verificar si ya existe una solicitud pendiente
            var existePendiente = await _repository.ExistePendienteAsync(request.ClienteId);
            if (existePendiente)
            {
                throw new InvalidOperationException("Ya existe una solicitud pendiente para este cliente");
            }

            // Obtener datos del cliente
            var cliente = await _clienteRepository.GetByIdAsync(request.ClienteId);
            if (cliente == null)
            {
                throw new InvalidOperationException("Cliente no encontrado");
            }

            // Crear la solicitud
            var solicitud = new Domain.Entities.SolicitudReventa
            {
                ClienteId = request.ClienteId,
                EmpresaId = request.EmpresaId,
                Cuit = request.Datos.Cuit,
                RazonSocial = request.Datos.RazonSocial,
                DireccionComercial = request.Datos.DireccionComercial,
                Localidad = request.Datos.Localidad,
                Provincia = request.Datos.Provincia,
                CodigoPostal = request.Datos.CodigoPostal,
                TelefonoComercial = request.Datos.TelefonoComercial,
                CategoriaIva = request.Datos.CategoriaIva,
                EmailComercial = request.Datos.EmailComercial,
                Estado = EstadoSolicitud.Pendiente,
                FechaSolicitud = DateTime.UtcNow
            };

            var solicitudCreada = await _repository.AddAsync(solicitud);

            // Enviar notificación por email
            try
            {
                await _notificationService.NotificarNuevaSolicitudReventaAsync(solicitudCreada, cliente);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar notificación de nueva solicitud de reventa");
            }

            return new SolicitudReventaDto
            {
                Id = solicitudCreada.Id,
                ClienteId = solicitudCreada.ClienteId,
                ClienteNombre = cliente.Nombre,
                ClienteEmail = cliente.Email,
                Cuit = solicitudCreada.Cuit,
                RazonSocial = solicitudCreada.RazonSocial,
                DireccionComercial = solicitudCreada.DireccionComercial,
                Localidad = solicitudCreada.Localidad,
                Provincia = solicitudCreada.Provincia,
                CodigoPostal = solicitudCreada.CodigoPostal,
                TelefonoComercial = solicitudCreada.TelefonoComercial,
                CategoriaIva = solicitudCreada.CategoriaIva,
                EmailComercial = solicitudCreada.EmailComercial,
                Estado = solicitudCreada.Estado.ToString(),
                FechaSolicitud = solicitudCreada.FechaSolicitud
            };
        }
    }
}