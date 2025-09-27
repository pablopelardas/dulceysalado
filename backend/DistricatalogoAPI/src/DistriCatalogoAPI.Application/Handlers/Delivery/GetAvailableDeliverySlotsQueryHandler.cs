using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using DistriCatalogoAPI.Application.Queries.Delivery;
using DistriCatalogoAPI.Application.DTOs;
using DistriCatalogoAPI.Domain.Interfaces;
using DistriCatalogoAPI.Domain.Enums;

namespace DistriCatalogoAPI.Application.Handlers.Delivery
{
    public class GetAvailableDeliverySlotsQueryHandler : IRequestHandler<GetAvailableDeliverySlotsQuery, List<AvailableDeliverySlotDto>>
    {
        private readonly IDeliveryRepository _deliveryRepository;

        public GetAvailableDeliverySlotsQueryHandler(IDeliveryRepository deliveryRepository)
        {
            _deliveryRepository = deliveryRepository;
        }

        public async Task<List<AvailableDeliverySlotDto>> Handle(GetAvailableDeliverySlotsQuery request, CancellationToken cancellationToken)
        {
            var settings = await _deliveryRepository.GetDeliverySettingsAsync(request.EmpresaId);
            if (settings == null)
            {
                return new List<AvailableDeliverySlotDto>();
            }

            // Usar la fecha solicitada o desde hoy
            var startDate = request.StartDate ?? DateOnly.FromDateTime(DateTime.Now);
            var endDate = request.EndDate ?? startDate.AddDays(30); // Por defecto próximos 30 días

            var availableSlots = new List<AvailableDeliverySlotDto>();

            // Obtener todas las franjas disponibles sin filtro de anticipación
            for (var currentDate = startDate; currentDate <= endDate; currentDate = currentDate.AddDays(1))
            {
                if (!IsValidDeliveryDay(currentDate, settings))
                    continue;

                var dailySlots = await GetDailySlotsAsync(currentDate, request.EmpresaId, settings, request.OnlyAvailable);

                if (dailySlots.MorningSlots.Any() || dailySlots.AfternoonSlots.Any())
                {
                    availableSlots.Add(dailySlots);
                }
            }

            // Aplicar filtro de franjas mínimas de anticipación
            return ApplyMinimumSlotsFilter(availableSlots, settings.MinSlotsAhead, DateTime.Now);
        }

        private bool IsValidDeliveryDay(DateOnly date, Domain.Entities.DeliverySettings settings)
        {
            return date.DayOfWeek switch
            {
                DayOfWeek.Monday => settings.MondayEnabled,
                DayOfWeek.Tuesday => settings.TuesdayEnabled,
                DayOfWeek.Wednesday => settings.WednesdayEnabled,
                DayOfWeek.Thursday => settings.ThursdayEnabled,
                DayOfWeek.Friday => settings.FridayEnabled,
                DayOfWeek.Saturday => settings.SaturdayEnabled,
                DayOfWeek.Sunday => settings.SundayEnabled,
                _ => false
            };
        }

        private async Task<AvailableDeliverySlotDto> GetDailySlotsAsync(
            DateOnly date,
            int empresaId,
            Domain.Entities.DeliverySettings settings,
            bool onlyAvailable)
        {
            var schedule = await _deliveryRepository.GetDeliveryScheduleAsync(settings.Id, date);
            var slots = await _deliveryRepository.GetDeliverySlotsAsync(settings.Id, date);

            var dailySlot = new AvailableDeliverySlotDto { Date = date };

            // Obtener horarios específicos para el día de la semana
            var (morningStart, morningEnd, afternoonStart, afternoonEnd, maxCapacityMorning, maxCapacityAfternoon) = 
                GetDaySpecificSchedule(date.DayOfWeek, settings, schedule);

            // Slots de mañana
            bool morningEnabled = schedule?.MorningEnabled ?? true; // Habilitado por defecto si no hay schedule específico

            if (morningEnabled && morningStart.HasValue && morningEnd.HasValue)
            {
                var morningSlot = slots.FirstOrDefault(s => s.SlotType == SlotType.Morning);
                var currentCapacity = morningSlot?.CurrentCapacity ?? 0;
                var isAvailable = currentCapacity < maxCapacityMorning;

                if (!onlyAvailable || isAvailable)
                {
                    dailySlot.MorningSlots.Add(new SlotAvailabilityDto
                    {
                        SlotType = SlotType.Morning,
                        SlotTypeName = "Mañana",
                        TimeRange = $"{morningStart:hh\\:mm} - {morningEnd:hh\\:mm}",
                        IsAvailable = isAvailable,
                        CurrentCapacity = currentCapacity,
                        MaxCapacity = maxCapacityMorning,
                        RemainingCapacity = maxCapacityMorning - currentCapacity
                    });
                }
            }

            // Slots de tarde
            bool afternoonEnabled = schedule?.AfternoonEnabled ?? true; // Habilitado por defecto si no hay schedule específico

            if (afternoonEnabled && afternoonStart.HasValue && afternoonEnd.HasValue)
            {
                var afternoonSlot = slots.FirstOrDefault(s => s.SlotType == SlotType.Afternoon);
                var currentCapacity = afternoonSlot?.CurrentCapacity ?? 0;
                var isAvailable = currentCapacity < maxCapacityAfternoon;

                if (!onlyAvailable || isAvailable)
                {
                    dailySlot.AfternoonSlots.Add(new SlotAvailabilityDto
                    {
                        SlotType = SlotType.Afternoon,
                        SlotTypeName = "Tarde",
                        TimeRange = $"{afternoonStart:hh\\:mm} - {afternoonEnd:hh\\:mm}",
                        IsAvailable = isAvailable,
                        CurrentCapacity = currentCapacity,
                        MaxCapacity = maxCapacityAfternoon,
                        RemainingCapacity = maxCapacityAfternoon - currentCapacity
                    });
                }
            }

            return dailySlot;
        }

        private (TimeSpan? morningStart, TimeSpan? morningEnd, TimeSpan? afternoonStart, TimeSpan? afternoonEnd, int maxCapacityMorning, int maxCapacityAfternoon) 
            GetDaySpecificSchedule(DayOfWeek dayOfWeek, Domain.Entities.DeliverySettings settings, Domain.Entities.DeliverySchedule? schedule)
        {
            // Si hay un schedule específico para esta fecha, usar sus configuraciones
            if (schedule != null)
            {
                var maxCapacityMorning = schedule.CustomMaxCapacityMorning ?? settings.MaxCapacityMorning;
                var maxCapacityAfternoon = schedule.CustomMaxCapacityAfternoon ?? settings.MaxCapacityAfternoon;

                // Si tiene horarios personalizados, usarlos; sino usar los por día de la semana
                var morningStart = schedule.CustomMorningStartTime ?? GetDayMorningStart(dayOfWeek, settings);
                var morningEnd = schedule.CustomMorningEndTime ?? GetDayMorningEnd(dayOfWeek, settings);
                var afternoonStart = schedule.CustomAfternoonStartTime ?? GetDayAfternoonStart(dayOfWeek, settings);
                var afternoonEnd = schedule.CustomAfternoonEndTime ?? GetDayAfternoonEnd(dayOfWeek, settings);

                return (morningStart, morningEnd, afternoonStart, afternoonEnd, maxCapacityMorning, maxCapacityAfternoon);
            }

            // Si no hay schedule específico, usar configuración por día de la semana
            var defaultCapacityMorning = settings.MaxCapacityMorning;
            var defaultCapacityAfternoon = settings.MaxCapacityAfternoon;

            return dayOfWeek switch
            {
                DayOfWeek.Monday => (settings.MondayMorningStart, settings.MondayMorningEnd, settings.MondayAfternoonStart, settings.MondayAfternoonEnd, defaultCapacityMorning, defaultCapacityAfternoon),
                DayOfWeek.Tuesday => (settings.TuesdayMorningStart, settings.TuesdayMorningEnd, settings.TuesdayAfternoonStart, settings.TuesdayAfternoonEnd, defaultCapacityMorning, defaultCapacityAfternoon),
                DayOfWeek.Wednesday => (settings.WednesdayMorningStart, settings.WednesdayMorningEnd, settings.WednesdayAfternoonStart, settings.WednesdayAfternoonEnd, defaultCapacityMorning, defaultCapacityAfternoon),
                DayOfWeek.Thursday => (settings.ThursdayMorningStart, settings.ThursdayMorningEnd, settings.ThursdayAfternoonStart, settings.ThursdayAfternoonEnd, defaultCapacityMorning, defaultCapacityAfternoon),
                DayOfWeek.Friday => (settings.FridayMorningStart, settings.FridayMorningEnd, settings.FridayAfternoonStart, settings.FridayAfternoonEnd, defaultCapacityMorning, defaultCapacityAfternoon),
                DayOfWeek.Saturday => (settings.SaturdayMorningStart, settings.SaturdayMorningEnd, settings.SaturdayAfternoonStart, settings.SaturdayAfternoonEnd, defaultCapacityMorning, defaultCapacityAfternoon),
                DayOfWeek.Sunday => (settings.SundayMorningStart, settings.SundayMorningEnd, settings.SundayAfternoonStart, settings.SundayAfternoonEnd, defaultCapacityMorning, defaultCapacityAfternoon),
                _ => (null, null, null, null, defaultCapacityMorning, defaultCapacityAfternoon)
            };
        }

        private TimeSpan? GetDayMorningStart(DayOfWeek dayOfWeek, Domain.Entities.DeliverySettings settings) => dayOfWeek switch
        {
            DayOfWeek.Monday => settings.MondayMorningStart,
            DayOfWeek.Tuesday => settings.TuesdayMorningStart,
            DayOfWeek.Wednesday => settings.WednesdayMorningStart,
            DayOfWeek.Thursday => settings.ThursdayMorningStart,
            DayOfWeek.Friday => settings.FridayMorningStart,
            DayOfWeek.Saturday => settings.SaturdayMorningStart,
            DayOfWeek.Sunday => settings.SundayMorningStart,
            _ => null
        };

        private TimeSpan? GetDayMorningEnd(DayOfWeek dayOfWeek, Domain.Entities.DeliverySettings settings) => dayOfWeek switch
        {
            DayOfWeek.Monday => settings.MondayMorningEnd,
            DayOfWeek.Tuesday => settings.TuesdayMorningEnd,
            DayOfWeek.Wednesday => settings.WednesdayMorningEnd,
            DayOfWeek.Thursday => settings.ThursdayMorningEnd,
            DayOfWeek.Friday => settings.FridayMorningEnd,
            DayOfWeek.Saturday => settings.SaturdayMorningEnd,
            DayOfWeek.Sunday => settings.SundayMorningEnd,
            _ => null
        };

        private TimeSpan? GetDayAfternoonStart(DayOfWeek dayOfWeek, Domain.Entities.DeliverySettings settings) => dayOfWeek switch
        {
            DayOfWeek.Monday => settings.MondayAfternoonStart,
            DayOfWeek.Tuesday => settings.TuesdayAfternoonStart,
            DayOfWeek.Wednesday => settings.WednesdayAfternoonStart,
            DayOfWeek.Thursday => settings.ThursdayAfternoonStart,
            DayOfWeek.Friday => settings.FridayAfternoonStart,
            DayOfWeek.Saturday => settings.SaturdayAfternoonStart,
            DayOfWeek.Sunday => settings.SundayAfternoonStart,
            _ => null
        };

        private TimeSpan? GetDayAfternoonEnd(DayOfWeek dayOfWeek, Domain.Entities.DeliverySettings settings) => dayOfWeek switch
        {
            DayOfWeek.Monday => settings.MondayAfternoonEnd,
            DayOfWeek.Tuesday => settings.TuesdayAfternoonEnd,
            DayOfWeek.Wednesday => settings.WednesdayAfternoonEnd,
            DayOfWeek.Thursday => settings.ThursdayAfternoonEnd,
            DayOfWeek.Friday => settings.FridayAfternoonEnd,
            DayOfWeek.Saturday => settings.SaturdayAfternoonEnd,
            DayOfWeek.Sunday => settings.SundayAfternoonEnd,
            _ => null
        };

        private List<AvailableDeliverySlotDto> ApplyMinimumSlotsFilter(List<AvailableDeliverySlotDto> allSlots, int minSlotsAhead, DateTime now)
        {
            if (minSlotsAhead <= 0)
                return allSlots;

            // Crear una lista plana de todas las franjas disponibles ordenadas cronológicamente
            var flatSlots = new List<(DateOnly Date, SlotType SlotType, SlotAvailabilityDto Slot)>();

            foreach (var daySlots in allSlots.OrderBy(d => d.Date))
            {
                // Agregar slots de mañana
                foreach (var morningSlot in daySlots.MorningSlots)
                {
                    flatSlots.Add((daySlots.Date, SlotType.Morning, morningSlot));
                }

                // Agregar slots de tarde
                foreach (var afternoonSlot in daySlots.AfternoonSlots)
                {
                    flatSlots.Add((daySlots.Date, SlotType.Afternoon, afternoonSlot));
                }
            }

            // Ordenar cronológicamente
            flatSlots = flatSlots.OrderBy(s => s.Date).ThenBy(s => (int)s.SlotType).ToList();

            // Encontrar el índice de la franja actual
            var currentTime = TimeOnly.FromDateTime(now);
            var currentDate = DateOnly.FromDateTime(now);
            var currentSlotType = currentTime < TimeOnly.Parse("13:00") ? SlotType.Morning : SlotType.Afternoon;

            var currentIndex = flatSlots.FindIndex(s => s.Date == currentDate && s.SlotType == currentSlotType);

            // Si no encontramos la franja actual, usar la primera disponible
            if (currentIndex == -1)
                currentIndex = 0;

            // Saltar las franjas mínimas requeridas
            var availableFromIndex = currentIndex + minSlotsAhead;

            if (availableFromIndex >= flatSlots.Count)
                return new List<AvailableDeliverySlotDto>(); // No hay franjas disponibles

            // Crear la respuesta con las franjas filtradas
            var filteredSlots = flatSlots.Skip(availableFromIndex).ToList();
            var result = new List<AvailableDeliverySlotDto>();

            foreach (var dateGroup in filteredSlots.GroupBy(s => s.Date))
            {
                var daySlot = new AvailableDeliverySlotDto { Date = dateGroup.Key };

                foreach (var slot in dateGroup)
                {
                    if (slot.SlotType == SlotType.Morning)
                        daySlot.MorningSlots.Add(slot.Slot);
                    else
                        daySlot.AfternoonSlots.Add(slot.Slot);
                }

                result.Add(daySlot);
            }

            return result;
        }
    }
}