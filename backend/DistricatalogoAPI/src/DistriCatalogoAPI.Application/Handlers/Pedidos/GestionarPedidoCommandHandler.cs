using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Application.Commands.Pedidos;
using DistriCatalogoAPI.Application.Interfaces;
using DistriCatalogoAPI.Domain.Interfaces;
using DistriCatalogoAPI.Domain.Entities;

namespace DistriCatalogoAPI.Application.Handlers.Pedidos
{
    public class GestionarPedidoCommandHandler : IRequestHandler<GestionarPedidoCommand, bool>
    {
        private readonly IPedidoRepository _pedidoRepository;
        private readonly INotificationService _notificationService;
        private readonly ILogger<GestionarPedidoCommandHandler> _logger;

        public GestionarPedidoCommandHandler(
            IPedidoRepository pedidoRepository,
            INotificationService notificationService,
            ILogger<GestionarPedidoCommandHandler> logger)
        {
            _pedidoRepository = pedidoRepository;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<bool> Handle(GestionarPedidoCommand request, CancellationToken cancellationToken)
        {
            try
            {
                // Obtener el pedido antes del cambio para las notificaciones
                var pedido = await _pedidoRepository.GetByIdAsync(request.PedidoId, true);
                if (pedido == null)
                {
                    _logger.LogWarning("Pedido {PedidoId} no encontrado", request.PedidoId);
                    return false;
                }

                // Guardar el estado anterior para notificaciones
                var estadoAnterior = pedido.Estado.ToString();
                
                var resultado = await _pedidoRepository.CambiarEstadoAsync(
                    request.PedidoId,
                    request.NuevoEstado,
                    request.UsuarioId,
                    request.Motivo);

                if (resultado)
                {
                    _logger.LogInformation("Pedido {PedidoId} cambió a estado {Estado} por usuario {UsuarioId}", 
                        request.PedidoId, request.NuevoEstado, request.UsuarioId);

                    // Recargar el pedido con el nuevo estado para las notificaciones
                    pedido = await _pedidoRepository.GetByIdAsync(request.PedidoId, true);

                    // Enviar notificaciones según el nuevo estado
                    try
                    {
                        switch (request.NuevoEstado)
                        {
                            case PedidoEstado.Cancelado:
                                // Notificar a usuarios internos sobre la cancelación
                                await _notificationService.NotificarCancelacionPedidoAsync(pedido, request.Motivo);
                                break;
                            
                            case PedidoEstado.Aceptado:
                            case PedidoEstado.Rechazado:
                            case PedidoEstado.Completado:
                                // Notificar al cliente sobre el cambio de estado
                                await _notificationService.NotificarCambioEstadoPedidoAsync(pedido, estadoAnterior);
                                break;
                        }
                        
                        _logger.LogInformation("Notificaciones enviadas para cambio de estado del pedido {PedidoId}", pedido.Id);
                    }
                    catch (Exception notificationEx)
                    {
                        _logger.LogError(notificationEx, "Error enviando notificaciones para pedido {PedidoId}", pedido.Id);
                        // No lanzamos la excepción para no afectar el cambio de estado
                    }
                }
                else
                {
                    _logger.LogWarning("No se pudo cambiar el estado del pedido {PedidoId} a {Estado}", 
                        request.PedidoId, request.NuevoEstado);
                }

                return resultado;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error gestionando pedido {PedidoId}", request.PedidoId);
                throw;
            }
        }
    }
}