using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using DistriCatalogoAPI.Application.Commands.Delivery;
using DistriCatalogoAPI.Application.DTOs;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.Delivery
{
    public class UpdateDeliverySettingsCommandHandler : IRequestHandler<UpdateDeliverySettingsCommand, DeliverySettingsDto>
    {
        private readonly IDeliveryRepository _deliveryRepository;
        private readonly IMapper _mapper;

        public UpdateDeliverySettingsCommandHandler(IDeliveryRepository deliveryRepository, IMapper mapper)
        {
            _deliveryRepository = deliveryRepository;
            _mapper = mapper;
        }

        public async Task<DeliverySettingsDto> Handle(UpdateDeliverySettingsCommand request, CancellationToken cancellationToken)
        {
            var settings = await _deliveryRepository.GetDeliverySettingsByIdAsync(request.Id);
            if (settings == null)
            {
                throw new ArgumentException($"No se encontraron configuraciones de delivery con ID {request.Id}");
            }

            // Actualizar configuración básica
            settings.UpdateCapacities(request.MaxCapacityMorning, request.MaxCapacityAfternoon);
            settings.UpdateMinSlotsAhead(request.MinSlotsAhead);

            // Actualizar días habilitados
            settings.SetWeeklySchedule(
                request.MondayEnabled,
                request.TuesdayEnabled,
                request.WednesdayEnabled,
                request.ThursdayEnabled,
                request.FridayEnabled,
                request.SaturdayEnabled,
                request.SundayEnabled
            );

            // Actualizar horarios por día
            settings.UpdateDaySchedules(
                // Lunes
                request.MondayMorningStart, request.MondayMorningEnd,
                request.MondayAfternoonStart, request.MondayAfternoonEnd,
                // Martes
                request.TuesdayMorningStart, request.TuesdayMorningEnd,
                request.TuesdayAfternoonStart, request.TuesdayAfternoonEnd,
                // Miércoles
                request.WednesdayMorningStart, request.WednesdayMorningEnd,
                request.WednesdayAfternoonStart, request.WednesdayAfternoonEnd,
                // Jueves
                request.ThursdayMorningStart, request.ThursdayMorningEnd,
                request.ThursdayAfternoonStart, request.ThursdayAfternoonEnd,
                // Viernes
                request.FridayMorningStart, request.FridayMorningEnd,
                request.FridayAfternoonStart, request.FridayAfternoonEnd,
                // Sábado
                request.SaturdayMorningStart, request.SaturdayMorningEnd,
                request.SaturdayAfternoonStart, request.SaturdayAfternoonEnd,
                // Domingo
                request.SundayMorningStart, request.SundayMorningEnd,
                request.SundayAfternoonStart, request.SundayAfternoonEnd
            );

            // Guardar cambios
            await _deliveryRepository.UpdateDeliverySettingsAsync(settings);
            await _deliveryRepository.SaveChangesAsync();

            // Mapear a DTO
            return _mapper.Map<DeliverySettingsDto>(settings);
        }
    }
}