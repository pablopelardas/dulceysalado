using MediatR;
using DistriCatalogoAPI.Application.DTOs;

namespace DistriCatalogoAPI.Application.Commands.Delivery
{
    public class UpdateDeliverySettingsCommand : IRequest<DeliverySettingsDto>
    {
        public int Id { get; set; }
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
}