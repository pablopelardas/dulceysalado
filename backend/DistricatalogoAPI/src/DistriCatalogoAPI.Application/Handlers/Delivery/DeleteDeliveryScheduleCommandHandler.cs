using MediatR;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Application.Commands.Delivery;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.Delivery
{
    public class DeleteDeliveryScheduleCommandHandler : IRequestHandler<DeleteDeliveryScheduleCommand>
    {
        private readonly IDeliveryRepository _deliveryRepository;
        private readonly ILogger<DeleteDeliveryScheduleCommandHandler> _logger;

        public DeleteDeliveryScheduleCommandHandler(
            IDeliveryRepository deliveryRepository,
            ILogger<DeleteDeliveryScheduleCommandHandler> logger)
        {
            _deliveryRepository = deliveryRepository;
            _logger = logger;
        }

        public async Task Handle(DeleteDeliveryScheduleCommand request, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Deleting delivery schedule {Id}", request.Id);

            // Verificar que el schedule existe
            var existingSchedule = await _deliveryRepository.GetDeliveryScheduleByIdAsync(request.Id);
            if (existingSchedule == null)
            {
                throw new ArgumentException($"Delivery schedule with ID {request.Id} not found");
            }

            // Eliminar el schedule
            await _deliveryRepository.DeleteDeliveryScheduleAsync(request.Id);
            await _deliveryRepository.SaveChangesAsync();

            _logger.LogDebug("Delivery schedule {Id} deleted successfully", request.Id);
        }
    }
}