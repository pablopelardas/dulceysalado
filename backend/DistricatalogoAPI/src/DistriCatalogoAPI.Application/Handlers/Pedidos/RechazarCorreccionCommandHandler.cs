using MediatR;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Application.Commands.Pedidos;
using DistriCatalogoAPI.Application.Interfaces;
using DistriCatalogoAPI.Domain.Interfaces;
using DistriCatalogoAPI.Domain.Entities;

namespace DistriCatalogoAPI.Application.Handlers.Pedidos
{
    public class RechazarCorreccionCommandHandler : IRequestHandler<RechazarCorreccionCommand, bool>
    {
        private readonly IPedidoRepository _pedidoRepository;
        private readonly INotificationService _notificationService;
        private readonly ILogger<RechazarCorreccionCommandHandler> _logger;

        public RechazarCorreccionCommandHandler(
            IPedidoRepository pedidoRepository,
            INotificationService notificationService,
            ILogger<RechazarCorreccionCommandHandler> logger)
        {
            _pedidoRepository = pedidoRepository;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<bool> Handle(RechazarCorreccionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var pedido = await _pedidoRepository.GetByIdWithItemsAsync(request.PedidoId);
                if (pedido == null)
                {
                    _logger.LogWarning("Pedido {PedidoId} no encontrado", request.PedidoId);
                    return false;
                }

                // Verificar que el pedido pertenece al cliente
                if (pedido.ClienteId != request.ClienteId)
                {
                    _logger.LogWarning("Cliente {ClienteId} no tiene permisos para rechazar corrección del pedido {PedidoId}", 
                        request.ClienteId, request.PedidoId);
                    return false;
                }

                // Verificar que el pedido está en estado de corrección pendiente
                if (pedido.Estado != PedidoEstado.CorreccionPendiente)
                {
                    _logger.LogWarning("Pedido {PedidoId} no está en estado de corrección pendiente. Estado actual: {Estado}", 
                        request.PedidoId, pedido.Estado);
                    return false;
                }

                // Rechazar la corrección
                pedido.RechazarCorreccion();
                await _pedidoRepository.UpdateAsync(pedido);

                _logger.LogInformation("Corrección del pedido {PedidoId} rechazada por cliente {ClienteId}", 
                    request.PedidoId, request.ClienteId);

                // Notificar a usuarios de la empresa sobre el rechazo
                try
                {
                    await _notificationService.NotificarRespuestaCorreccionAsync(pedido, false, request.Comentario);
                }
                catch (Exception notificationEx)
                {
                    _logger.LogError(notificationEx, "Error enviando notificaciones de rechazo para pedido {PedidoId}", pedido.Id);
                    // No lanzamos la excepción para no afectar el rechazo
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rechazando corrección del pedido {PedidoId}", request.PedidoId);
                return false;
            }
        }
    }
}