using System;
using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using DistriCatalogoAPI.Application.Commands.Delivery;
using DistriCatalogoAPI.Application.DTOs;
using DistriCatalogoAPI.Domain.Entities;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.Delivery
{
    public class CreateDeliverySettingsCommandHandler : IRequestHandler<CreateDeliverySettingsCommand, DeliverySettingsDto>
    {
        private readonly IDeliveryRepository _deliveryRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IMapper _mapper;

        public CreateDeliverySettingsCommandHandler(
            IDeliveryRepository deliveryRepository,
            ICompanyRepository companyRepository,
            IMapper mapper)
        {
            _deliveryRepository = deliveryRepository;
            _companyRepository = companyRepository;
            _mapper = mapper;
        }

        public async Task<DeliverySettingsDto> Handle(CreateDeliverySettingsCommand request, CancellationToken cancellationToken)
        {
            // Validar que la empresa existe
            var empresa = await _companyRepository.GetByIdAsync(request.EmpresaId);
            if (empresa == null)
            {
                throw new ArgumentException($"La empresa con ID {request.EmpresaId} no existe");
            }

            // Validar que no exista ya una configuración de delivery para esta empresa
            var existingSettings = await _deliveryRepository.GetDeliverySettingsAsync(request.EmpresaId);
            if (existingSettings != null)
            {
                throw new InvalidOperationException($"Ya existe una configuración de delivery para la empresa {request.EmpresaId}");
            }

            // Crear nueva configuración
            var deliverySettings = new DeliverySettings(
                request.EmpresaId,
                request.MinSlotsAhead,
                request.MaxCapacityMorning,
                request.MaxCapacityAfternoon
            );

            // Configurar días habilitados
            deliverySettings.SetWeeklySchedule(
                request.MondayEnabled,
                request.TuesdayEnabled,
                request.WednesdayEnabled,
                request.ThursdayEnabled,
                request.FridayEnabled,
                request.SaturdayEnabled,
                request.SundayEnabled
            );

            // Configurar horarios por día
            deliverySettings.UpdateDaySchedules(
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

            // Guardar en repositorio
            var created = await _deliveryRepository.CreateDeliverySettingsAsync(deliverySettings);
            await _deliveryRepository.SaveChangesAsync();

            // Mapear a DTO
            return _mapper.Map<DeliverySettingsDto>(created);
        }
    }
}