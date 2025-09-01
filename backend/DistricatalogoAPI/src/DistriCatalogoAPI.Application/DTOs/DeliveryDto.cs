using System;
using System.Collections.Generic;
using DistriCatalogoAPI.Domain.Enums;

namespace DistriCatalogoAPI.Application.DTOs
{
    public class DeliverySettingsDto
    {
        public int Id { get; set; }
        public int EmpresaId { get; set; }
        public int MinSlotsAhead { get; set; }
        public int MaxCapacityMorning { get; set; }
        public int MaxCapacityAfternoon { get; set; }
        
        // Configuración por día - Lunes
        public bool MondayEnabled { get; set; }
        public TimeSpan? MondayMorningStart { get; set; }
        public TimeSpan? MondayMorningEnd { get; set; }
        public TimeSpan? MondayAfternoonStart { get; set; }
        public TimeSpan? MondayAfternoonEnd { get; set; }
        
        // Configuración por día - Martes
        public bool TuesdayEnabled { get; set; }
        public TimeSpan? TuesdayMorningStart { get; set; }
        public TimeSpan? TuesdayMorningEnd { get; set; }
        public TimeSpan? TuesdayAfternoonStart { get; set; }
        public TimeSpan? TuesdayAfternoonEnd { get; set; }
        
        // Configuración por día - Miércoles
        public bool WednesdayEnabled { get; set; }
        public TimeSpan? WednesdayMorningStart { get; set; }
        public TimeSpan? WednesdayMorningEnd { get; set; }
        public TimeSpan? WednesdayAfternoonStart { get; set; }
        public TimeSpan? WednesdayAfternoonEnd { get; set; }
        
        // Configuración por día - Jueves
        public bool ThursdayEnabled { get; set; }
        public TimeSpan? ThursdayMorningStart { get; set; }
        public TimeSpan? ThursdayMorningEnd { get; set; }
        public TimeSpan? ThursdayAfternoonStart { get; set; }
        public TimeSpan? ThursdayAfternoonEnd { get; set; }
        
        // Configuración por día - Viernes
        public bool FridayEnabled { get; set; }
        public TimeSpan? FridayMorningStart { get; set; }
        public TimeSpan? FridayMorningEnd { get; set; }
        public TimeSpan? FridayAfternoonStart { get; set; }
        public TimeSpan? FridayAfternoonEnd { get; set; }
        
        // Configuración por día - Sábado
        public bool SaturdayEnabled { get; set; }
        public TimeSpan? SaturdayMorningStart { get; set; }
        public TimeSpan? SaturdayMorningEnd { get; set; }
        public TimeSpan? SaturdayAfternoonStart { get; set; }
        public TimeSpan? SaturdayAfternoonEnd { get; set; }
        
        // Configuración por día - Domingo
        public bool SundayEnabled { get; set; }
        public TimeSpan? SundayMorningStart { get; set; }
        public TimeSpan? SundayMorningEnd { get; set; }
        public TimeSpan? SundayAfternoonStart { get; set; }
        public TimeSpan? SundayAfternoonEnd { get; set; }
        
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<DeliveryScheduleDto> Schedules { get; set; } = new List<DeliveryScheduleDto>();
        public List<DeliverySlotDto> Slots { get; set; } = new List<DeliverySlotDto>();
    }

    public class DeliveryScheduleDto
    {
        public int Id { get; set; }
        public int DeliverySettingsId { get; set; }
        public DateOnly Date { get; set; }
        public bool MorningEnabled { get; set; }
        public bool AfternoonEnabled { get; set; }
        public int? CustomMaxCapacityMorning { get; set; }
        public int? CustomMaxCapacityAfternoon { get; set; }
        public TimeSpan? CustomMorningStartTime { get; set; }
        public TimeSpan? CustomMorningEndTime { get; set; }
        public TimeSpan? CustomAfternoonStartTime { get; set; }
        public TimeSpan? CustomAfternoonEndTime { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<DeliverySlotDto> Slots { get; set; } = new List<DeliverySlotDto>();
    }

    public class DeliverySlotDto
    {
        public int Id { get; set; }
        public int DeliverySettingsId { get; set; }
        public int? DeliveryScheduleId { get; set; }
        public DateOnly Date { get; set; }
        public SlotType SlotType { get; set; }
        public string SlotTypeName { get; set; }
        public int CurrentCapacity { get; set; }
        public int MaxCapacity { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class CreateDeliverySettingsDto
    {
        public int EmpresaId { get; set; }
        public int MinSlotsAhead { get; set; } = 2;
        public int MaxCapacityMorning { get; set; } = 10;
        public int MaxCapacityAfternoon { get; set; } = 10;
        
        // Configuración por día - Lunes
        public bool MondayEnabled { get; set; } = true;
        public TimeSpan? MondayMorningStart { get; set; } = new TimeSpan(9, 0, 0);
        public TimeSpan? MondayMorningEnd { get; set; } = new TimeSpan(13, 0, 0);
        public TimeSpan? MondayAfternoonStart { get; set; } = new TimeSpan(14, 0, 0);
        public TimeSpan? MondayAfternoonEnd { get; set; } = new TimeSpan(18, 0, 0);
        
        // Configuración por día - Martes
        public bool TuesdayEnabled { get; set; } = true;
        public TimeSpan? TuesdayMorningStart { get; set; } = new TimeSpan(9, 0, 0);
        public TimeSpan? TuesdayMorningEnd { get; set; } = new TimeSpan(13, 0, 0);
        public TimeSpan? TuesdayAfternoonStart { get; set; } = new TimeSpan(14, 0, 0);
        public TimeSpan? TuesdayAfternoonEnd { get; set; } = new TimeSpan(18, 0, 0);
        
        // Configuración por día - Miércoles
        public bool WednesdayEnabled { get; set; } = true;
        public TimeSpan? WednesdayMorningStart { get; set; } = new TimeSpan(9, 0, 0);
        public TimeSpan? WednesdayMorningEnd { get; set; } = new TimeSpan(13, 0, 0);
        public TimeSpan? WednesdayAfternoonStart { get; set; } = new TimeSpan(14, 0, 0);
        public TimeSpan? WednesdayAfternoonEnd { get; set; } = new TimeSpan(18, 0, 0);
        
        // Configuración por día - Jueves
        public bool ThursdayEnabled { get; set; } = true;
        public TimeSpan? ThursdayMorningStart { get; set; } = new TimeSpan(9, 0, 0);
        public TimeSpan? ThursdayMorningEnd { get; set; } = new TimeSpan(13, 0, 0);
        public TimeSpan? ThursdayAfternoonStart { get; set; } = new TimeSpan(14, 0, 0);
        public TimeSpan? ThursdayAfternoonEnd { get; set; } = new TimeSpan(18, 0, 0);
        
        // Configuración por día - Viernes
        public bool FridayEnabled { get; set; } = true;
        public TimeSpan? FridayMorningStart { get; set; } = new TimeSpan(9, 0, 0);
        public TimeSpan? FridayMorningEnd { get; set; } = new TimeSpan(13, 0, 0);
        public TimeSpan? FridayAfternoonStart { get; set; } = new TimeSpan(14, 0, 0);
        public TimeSpan? FridayAfternoonEnd { get; set; } = new TimeSpan(18, 0, 0);
        
        // Configuración por día - Sábado
        public bool SaturdayEnabled { get; set; } = false;
        public TimeSpan? SaturdayMorningStart { get; set; } = new TimeSpan(9, 0, 0);
        public TimeSpan? SaturdayMorningEnd { get; set; } = new TimeSpan(12, 0, 0);
        public TimeSpan? SaturdayAfternoonStart { get; set; } = null;
        public TimeSpan? SaturdayAfternoonEnd { get; set; } = null;
        
        // Configuración por día - Domingo
        public bool SundayEnabled { get; set; } = false;
        public TimeSpan? SundayMorningStart { get; set; } = null;
        public TimeSpan? SundayMorningEnd { get; set; } = null;
        public TimeSpan? SundayAfternoonStart { get; set; } = null;
        public TimeSpan? SundayAfternoonEnd { get; set; } = null;
    }

    public class UpdateDeliverySettingsDto
    {
        public int MinSlotsAhead { get; set; }
        public int MaxCapacityMorning { get; set; }
        public int MaxCapacityAfternoon { get; set; }
        
        // Configuración por día - Lunes
        public bool MondayEnabled { get; set; }
        public TimeSpan? MondayMorningStart { get; set; }
        public TimeSpan? MondayMorningEnd { get; set; }
        public TimeSpan? MondayAfternoonStart { get; set; }
        public TimeSpan? MondayAfternoonEnd { get; set; }
        
        // Configuración por día - Martes
        public bool TuesdayEnabled { get; set; }
        public TimeSpan? TuesdayMorningStart { get; set; }
        public TimeSpan? TuesdayMorningEnd { get; set; }
        public TimeSpan? TuesdayAfternoonStart { get; set; }
        public TimeSpan? TuesdayAfternoonEnd { get; set; }
        
        // Configuración por día - Miércoles
        public bool WednesdayEnabled { get; set; }
        public TimeSpan? WednesdayMorningStart { get; set; }
        public TimeSpan? WednesdayMorningEnd { get; set; }
        public TimeSpan? WednesdayAfternoonStart { get; set; }
        public TimeSpan? WednesdayAfternoonEnd { get; set; }
        
        // Configuración por día - Jueves
        public bool ThursdayEnabled { get; set; }
        public TimeSpan? ThursdayMorningStart { get; set; }
        public TimeSpan? ThursdayMorningEnd { get; set; }
        public TimeSpan? ThursdayAfternoonStart { get; set; }
        public TimeSpan? ThursdayAfternoonEnd { get; set; }
        
        // Configuración por día - Viernes
        public bool FridayEnabled { get; set; }
        public TimeSpan? FridayMorningStart { get; set; }
        public TimeSpan? FridayMorningEnd { get; set; }
        public TimeSpan? FridayAfternoonStart { get; set; }
        public TimeSpan? FridayAfternoonEnd { get; set; }
        
        // Configuración por día - Sábado
        public bool SaturdayEnabled { get; set; }
        public TimeSpan? SaturdayMorningStart { get; set; }
        public TimeSpan? SaturdayMorningEnd { get; set; }
        public TimeSpan? SaturdayAfternoonStart { get; set; }
        public TimeSpan? SaturdayAfternoonEnd { get; set; }
        
        // Configuración por día - Domingo
        public bool SundayEnabled { get; set; }
        public TimeSpan? SundayMorningStart { get; set; }
        public TimeSpan? SundayMorningEnd { get; set; }
        public TimeSpan? SundayAfternoonStart { get; set; }
        public TimeSpan? SundayAfternoonEnd { get; set; }
    }

    public class CreateDeliveryScheduleDto
    {
        public DateOnly Date { get; set; }
        public bool MorningEnabled { get; set; } = true;
        public bool AfternoonEnabled { get; set; } = true;
        public int? CustomMaxCapacityMorning { get; set; }
        public int? CustomMaxCapacityAfternoon { get; set; }
        public TimeSpan? CustomMorningStartTime { get; set; }
        public TimeSpan? CustomMorningEndTime { get; set; }
        public TimeSpan? CustomAfternoonStartTime { get; set; }
        public TimeSpan? CustomAfternoonEndTime { get; set; }
    }

    public class UpdateDeliveryScheduleDto
    {
        public bool MorningEnabled { get; set; }
        public bool AfternoonEnabled { get; set; }
        public int? CustomMaxCapacityMorning { get; set; }
        public int? CustomMaxCapacityAfternoon { get; set; }
        public TimeSpan? CustomMorningStartTime { get; set; }
        public TimeSpan? CustomMorningEndTime { get; set; }
        public TimeSpan? CustomAfternoonStartTime { get; set; }
        public TimeSpan? CustomAfternoonEndTime { get; set; }
    }

    public class AvailableDeliverySlotDto
    {
        public DateOnly Date { get; set; }
        public List<SlotAvailabilityDto> MorningSlots { get; set; } = new List<SlotAvailabilityDto>();
        public List<SlotAvailabilityDto> AfternoonSlots { get; set; } = new List<SlotAvailabilityDto>();
    }

    public class SlotAvailabilityDto
    {
        public SlotType SlotType { get; set; }
        public string SlotTypeName { get; set; }
        public string TimeRange { get; set; }
        public bool IsAvailable { get; set; }
        public int CurrentCapacity { get; set; }
        public int MaxCapacity { get; set; }
        public int RemainingCapacity { get; set; }
    }

    public class DeliverySlotsQueryDto
    {
        public DateOnly? StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public SlotType? SlotType { get; set; }
        public bool? OnlyAvailable { get; set; } = true;
    }
}