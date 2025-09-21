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
    public class ResponderSolicitudReventaCommandHandler : IRequestHandler<ResponderSolicitudReventaCommand, SolicitudReventaDto>
    {
        private readonly ISolicitudReventaRepository _repository;
        private readonly IClienteRepository _clienteRepository;
        private readonly IListaPrecioRepository _listaPrecioRepository;
        private readonly ILogger<ResponderSolicitudReventaCommandHandler> _logger;
        private readonly INotificationService _notificationService;

        public ResponderSolicitudReventaCommandHandler(
            ISolicitudReventaRepository repository,
            IClienteRepository clienteRepository,
            IListaPrecioRepository listaPrecioRepository,
            ILogger<ResponderSolicitudReventaCommandHandler> logger,
            INotificationService notificationService)
        {
            _repository = repository;
            _clienteRepository = clienteRepository;
            _listaPrecioRepository = listaPrecioRepository;
            _logger = logger;
            _notificationService = notificationService;
        }

        public async Task<SolicitudReventaDto> Handle(ResponderSolicitudReventaCommand request, CancellationToken cancellationToken)
        {
            var solicitud = await _repository.GetByIdAsync(request.SolicitudId);
            if (solicitud == null)
            {
                throw new InvalidOperationException("Solicitud no encontrada");
            }

            if (solicitud.Estado == EstadoSolicitud.Aprobada)
            {
                throw new InvalidOperationException("La solicitud ya está aprobada");
            }

            // Actualizar el estado de la solicitud
            solicitud.Estado = request.Respuesta.Aprobar ? EstadoSolicitud.Aprobada : EstadoSolicitud.Rechazada;
            solicitud.ComentarioRespuesta = request.Respuesta.ComentarioRespuesta;
            solicitud.FechaRespuesta = DateTime.UtcNow;
            solicitud.RespondidoPor = request.RespondidoPor;

            // Si se aprueba, actualizar la lista de precios del cliente
            if (request.Respuesta.Aprobar)
            {
                var cliente = await _clienteRepository.GetByIdAsync(solicitud.ClienteId);
                if (cliente != null)
                {
                    // Buscar el ID de la lista de precios con código "2"
                    var listaPrecioId = await _listaPrecioRepository.GetIdByCodigoAsync("2");
                    if (listaPrecioId.HasValue)
                    {
                        cliente.ListaPrecioId = listaPrecioId.Value;
                        await _clienteRepository.UpdateAsync(cliente);
                    }
                    else
                    {
                        _logger.LogWarning("No se encontró la lista de precios con código '2' para asignar al cliente {ClienteId}", cliente.Id);
                    }
                }
            }

            await _repository.UpdateAsync(solicitud);

            // Enviar notificación por email
            try
            {
                var cliente = await _clienteRepository.GetByIdAsync(solicitud.ClienteId);
                if (cliente != null)
                {
                    await _notificationService.NotificarRespuestaSolicitudReventaAsync(solicitud, cliente);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar notificación de respuesta de solicitud de reventa");
            }

            var clienteData = solicitud.Cliente ?? await _clienteRepository.GetByIdAsync(solicitud.ClienteId);

            return new SolicitudReventaDto
            {
                Id = solicitud.Id,
                ClienteId = solicitud.ClienteId,
                ClienteNombre = clienteData?.Nombre,
                ClienteEmail = clienteData?.Email,
                Cuit = solicitud.Cuit,
                RazonSocial = solicitud.RazonSocial,
                DireccionComercial = solicitud.DireccionComercial,
                Localidad = solicitud.Localidad,
                Provincia = solicitud.Provincia,
                CodigoPostal = solicitud.CodigoPostal,
                TelefonoComercial = solicitud.TelefonoComercial,
                CategoriaIva = solicitud.CategoriaIva,
                EmailComercial = solicitud.EmailComercial,
                Estado = solicitud.Estado.ToString(),
                ComentarioRespuesta = solicitud.ComentarioRespuesta,
                FechaRespuesta = solicitud.FechaRespuesta,
                RespondidoPor = solicitud.RespondidoPor,
                FechaSolicitud = solicitud.FechaSolicitud
            };
        }
    }
}