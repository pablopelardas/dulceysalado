using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using DistriCatalogoAPI.Application.Commands.Delivery;
using DistriCatalogoAPI.Application.DTOs;
using DistriCatalogoAPI.Domain.Entities;
using DistriCatalogoAPI.Domain.Interfaces;
using Microsoft.Extensions.Logging;

namespace DistriCatalogoAPI.Application.Handlers.Delivery
{
    public class CreateDeliveryScheduleCommandHandler : IRequestHandler<CreateDeliveryScheduleCommand, DeliveryScheduleDto>
    {
        private readonly IDeliveryRepository _deliveryRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<CreateDeliveryScheduleCommandHandler> _logger;

        public CreateDeliveryScheduleCommandHandler(
            IDeliveryRepository deliveryRepository,
            IMapper mapper,
            ILogger<CreateDeliveryScheduleCommandHandler> logger)
        {
            _deliveryRepository = deliveryRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<DeliveryScheduleDto> Handle(CreateDeliveryScheduleCommand request, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Creating delivery schedule for settings {DeliverySettingsId}, date {Date}",
                request.DeliverySettingsId, request.Date);

            // Verificar si ya existe un schedule para esta fecha
            var existingSchedule = await _deliveryRepository.GetDeliveryScheduleAsync(
                request.DeliverySettingsId, 
                request.Date
            );

            if (existingSchedule != null)
            {
                throw new InvalidOperationException(
                    $"Ya existe un horario de entrega para la fecha {request.Date:yyyy-MM-dd}");
            }

            // Crear nuevo schedule
            var schedule = new DeliverySchedule(
                request.DeliverySettingsId,
                request.Date,
                request.MorningEnabled,
                request.AfternoonEnabled,
                request.CustomMaxCapacityMorning,
                request.CustomMaxCapacityAfternoon,
                request.CustomMorningStartTime,
                request.CustomMorningEndTime,
                request.CustomAfternoonStartTime,
                request.CustomAfternoonEndTime
            );

            // Guardar en repositorio
            var created = await _deliveryRepository.CreateDeliveryScheduleAsync(schedule);
            await _deliveryRepository.SaveChangesAsync();

            _logger.LogInformation("Delivery schedule created successfully for date {Date}", request.Date);

            // Mapear a DTO
            return _mapper.Map<DeliveryScheduleDto>(created);
        }
    }
}