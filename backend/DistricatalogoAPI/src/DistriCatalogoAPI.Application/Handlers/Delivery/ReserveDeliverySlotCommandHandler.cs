using System.Threading;
using System.Threading.Tasks;
using MediatR;
using DistriCatalogoAPI.Application.Commands.Delivery;
using DistriCatalogoAPI.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace DistriCatalogoAPI.Application.Handlers.Delivery
{
    public class ReserveDeliverySlotCommandHandler : IRequestHandler<ReserveDeliverySlotCommand, bool>
    {
        private readonly IDeliveryRepository _deliveryRepository;
        private readonly ILogger<ReserveDeliverySlotCommandHandler> _logger;

        public ReserveDeliverySlotCommandHandler(
            IDeliveryRepository deliveryRepository,
            ILogger<ReserveDeliverySlotCommandHandler> logger)
        {
            _deliveryRepository = deliveryRepository;
            _logger = logger;
        }

        public async Task<bool> Handle(ReserveDeliverySlotCommand request, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Reserving slot for empresa {EmpresaId}, date {Date}, type {SlotType}, pedido {PedidoId}",
                request.EmpresaId, request.Date, request.SlotType, request.PedidoId);

            var result = await _deliveryRepository.ReserveSlotAsync(
                request.EmpresaId,
                request.Date,
                request.SlotType
            );

            if (result)
            {
                await _deliveryRepository.SaveChangesAsync();
                _logger.LogInformation("Slot reserved successfully for pedido {PedidoId}", request.PedidoId);
            }
            else
            {
                _logger.LogWarning("Failed to reserve slot for pedido {PedidoId}", request.PedidoId);
            }

            return result;
        }
    }
}