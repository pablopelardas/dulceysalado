using MediatR;
using DistriCatalogoAPI.Application.DTOs;

namespace DistriCatalogoAPI.Application.Commands.Delivery
{
    public class CreateDeliverySettingsCommand : IRequest<DeliverySettingsDto>
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
}