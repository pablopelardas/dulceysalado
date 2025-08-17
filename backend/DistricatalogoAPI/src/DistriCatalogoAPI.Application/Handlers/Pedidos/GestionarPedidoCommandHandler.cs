using System;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Application.Commands.Pedidos;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.Pedidos
{
    public class GestionarPedidoCommandHandler : IRequestHandler<GestionarPedidoCommand, bool>
    {
        private readonly IPedidoRepository _pedidoRepository;
        private readonly ILogger<GestionarPedidoCommandHandler> _logger;

        public GestionarPedidoCommandHandler(
            IPedidoRepository pedidoRepository,
            ILogger<GestionarPedidoCommandHandler> logger)
        {
            _pedidoRepository = pedidoRepository;
            _logger = logger;
        }

        public async Task<bool> Handle(GestionarPedidoCommand request, CancellationToken cancellationToken)
        {
            try
            {
                var resultado = await _pedidoRepository.CambiarEstadoAsync(
                    request.PedidoId,
                    request.NuevoEstado,
                    request.UsuarioId,
                    request.Motivo);

                if (resultado)
                {
                    _logger.LogInformation("Pedido {PedidoId} cambió a estado {Estado} por usuario {UsuarioId}", 
                        request.PedidoId, request.NuevoEstado, request.UsuarioId);
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