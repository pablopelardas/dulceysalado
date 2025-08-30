using MediatR;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Application.Commands.Pedidos;
using DistriCatalogoAPI.Application.Interfaces;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.Pedidos
{
    public class ResponderCorreccionCommandHandler : IRequestHandler<ResponderCorreccionCommand, bool>
    {
        private readonly ICorreccionTokenRepository _tokenRepository;
        private readonly IPedidoRepository _pedidoRepository;
        private readonly INotificationService _notificationService;
        private readonly ILogger<ResponderCorreccionCommandHandler> _logger;

        public ResponderCorreccionCommandHandler(
            ICorreccionTokenRepository tokenRepository,
            IPedidoRepository pedidoRepository,
            INotificationService notificationService,
            ILogger<ResponderCorreccionCommandHandler> logger)
        {
            _tokenRepository = tokenRepository;
            _pedidoRepository = pedidoRepository;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<bool> Handle(ResponderCorreccionCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var token = await _tokenRepository.GetByTokenAsync(request.Token);
                if (token == null || !token.EsValido())
                {
                    _logger.LogWarning("Token de corrección inválido o expirado: {Token}", request.Token);
                    return false;
                }

                var pedido = await _pedidoRepository.GetByIdAsync(token.PedidoId);
                if (pedido == null)
                {
                    _logger.LogWarning("Pedido {PedidoId} no encontrado para token {Token}", 
                        token.PedidoId, request.Token);
                    return false;
                }

                // Marcar token como usado
                var respuesta = request.Aprobado ? "Aprobado" : "Rechazado";
                token.MarcarComoUsado(respuesta, request.ComentarioCliente);

                // Actualizar estado del pedido
                if (request.Aprobado)
                {
                    pedido.AprobarCorreccion();
                    _logger.LogInformation("Corrección aprobada para pedido {PedidoId}", pedido.Id);
                }
                else
                {
                    pedido.RechazarCorreccion();
                    _logger.LogInformation("Corrección rechazada para pedido {PedidoId}", pedido.Id);
                }

                await _tokenRepository.UpdateAsync(token);
                await _pedidoRepository.UpdateAsync(pedido);

                // Notificar a usuarios de la empresa sobre la respuesta
                try
                {
                    await _notificationService.NotificarRespuestaCorreccionAsync(
                        pedido, request.Aprobado, request.ComentarioCliente);
                    _logger.LogInformation("Notificaciones enviadas para respuesta de corrección del pedido {PedidoId}", 
                        pedido.Id);
                }
                catch (Exception notificationEx)
                {
                    _logger.LogError(notificationEx, 
                        "Error enviando notificaciones de respuesta para pedido {PedidoId}", pedido.Id);
                    // No lanzamos la excepción para no afectar la respuesta
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error procesando respuesta de corrección para token {Token}", request.Token);
                return false;
            }
        }
    }
}