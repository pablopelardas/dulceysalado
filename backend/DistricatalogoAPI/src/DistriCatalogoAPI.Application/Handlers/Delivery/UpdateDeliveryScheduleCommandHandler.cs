using MediatR;
using AutoMapper;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Application.Commands.Delivery;
using DistriCatalogoAPI.Application.DTOs;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.Delivery
{
    public class UpdateDeliveryScheduleCommandHandler : IRequestHandler<UpdateDeliveryScheduleCommand, DeliveryScheduleDto>
    {
        private readonly IDeliveryRepository _deliveryRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<UpdateDeliveryScheduleCommandHandler> _logger;

        public UpdateDeliveryScheduleCommandHandler(
            IDeliveryRepository deliveryRepository,
            IMapper mapper,
            ILogger<UpdateDeliveryScheduleCommandHandler> logger)
        {
            _deliveryRepository = deliveryRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<DeliveryScheduleDto> Handle(UpdateDeliveryScheduleCommand request, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Updating delivery schedule {Id}", request.Id);

            // Buscar el schedule existente
            var existingSchedule = await _deliveryRepository.GetDeliveryScheduleByIdAsync(request.Id);
            if (existingSchedule == null)
            {
                throw new ArgumentException($"Delivery schedule with ID {request.Id} not found");
            }

            // Actualizar propiedades
            existingSchedule.MorningEnabled = request.MorningEnabled;
            existingSchedule.AfternoonEnabled = request.AfternoonEnabled;
            existingSchedule.CustomMaxCapacityMorning = request.CustomMaxCapacityMorning;
            existingSchedule.CustomMaxCapacityAfternoon = request.CustomMaxCapacityAfternoon;
            existingSchedule.CustomMorningStartTime = request.CustomMorningStartTime;
            existingSchedule.CustomMorningEndTime = request.CustomMorningEndTime;
            existingSchedule.CustomAfternoonStartTime = request.CustomAfternoonStartTime;
            existingSchedule.CustomAfternoonEndTime = request.CustomAfternoonEndTime;
            existingSchedule.UpdatedAt = DateTime.UtcNow;

            // Guardar cambios
            await _deliveryRepository.UpdateDeliveryScheduleAsync(existingSchedule);
            await _deliveryRepository.SaveChangesAsync();

            _logger.LogDebug("Delivery schedule {Id} updated successfully", request.Id);

            return _mapper.Map<DeliveryScheduleDto>(existingSchedule);
        }
    }
}