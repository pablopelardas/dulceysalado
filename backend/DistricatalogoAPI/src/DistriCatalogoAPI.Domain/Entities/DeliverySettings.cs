namespace DistriCatalogoAPI.Domain.Entities
{
    public class DeliverySettings
    {
        public int Id { get; set; }
        
        public int EmpresaId { get; set; }
        
        public int MinSlotsAhead { get; set; } = 2;
        
        // Capacidades generales por franja
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
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        public virtual Company Empresa { get; set; } = null!;
        public virtual ICollection<DeliverySchedule> Schedules { get; set; } = new List<DeliverySchedule>();
        public virtual ICollection<DeliverySlot> Slots { get; set; } = new List<DeliverySlot>();

        // Constructor
        public DeliverySettings() { }

        public DeliverySettings(int empresaId, int minSlotsAhead, int maxCapacityMorning, int maxCapacityAfternoon)
        {
            EmpresaId = empresaId;
            MinSlotsAhead = minSlotsAhead;
            MaxCapacityMorning = maxCapacityMorning;
            MaxCapacityAfternoon = maxCapacityAfternoon;
        }

        // Métodos de negocio
        public void SetId(int id)
        {
            Id = id;
        }

        public void SetWeeklySchedule(
            bool monday, bool tuesday, bool wednesday, bool thursday,
            bool friday, bool saturday, bool sunday)
        {
            MondayEnabled = monday;
            TuesdayEnabled = tuesday;
            WednesdayEnabled = wednesday;
            ThursdayEnabled = thursday;
            FridayEnabled = friday;
            SaturdayEnabled = saturday;
            SundayEnabled = sunday;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateCapacities(int maxCapacityMorning, int maxCapacityAfternoon)
        {
            MaxCapacityMorning = maxCapacityMorning;
            MaxCapacityAfternoon = maxCapacityAfternoon;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateMinSlotsAhead(int minSlotsAhead)
        {
            MinSlotsAhead = minSlotsAhead;
            UpdatedAt = DateTime.UtcNow;
        }

        public void UpdateDaySchedules(
            // Lunes
            TimeSpan? mondayMorningStart, TimeSpan? mondayMorningEnd,
            TimeSpan? mondayAfternoonStart, TimeSpan? mondayAfternoonEnd,
            // Martes
            TimeSpan? tuesdayMorningStart, TimeSpan? tuesdayMorningEnd,
            TimeSpan? tuesdayAfternoonStart, TimeSpan? tuesdayAfternoonEnd,
            // Miércoles
            TimeSpan? wednesdayMorningStart, TimeSpan? wednesdayMorningEnd,
            TimeSpan? wednesdayAfternoonStart, TimeSpan? wednesdayAfternoonEnd,
            // Jueves
            TimeSpan? thursdayMorningStart, TimeSpan? thursdayMorningEnd,
            TimeSpan? thursdayAfternoonStart, TimeSpan? thursdayAfternoonEnd,
            // Viernes
            TimeSpan? fridayMorningStart, TimeSpan? fridayMorningEnd,
            TimeSpan? fridayAfternoonStart, TimeSpan? fridayAfternoonEnd,
            // Sábado
            TimeSpan? saturdayMorningStart, TimeSpan? saturdayMorningEnd,
            TimeSpan? saturdayAfternoonStart, TimeSpan? saturdayAfternoonEnd,
            // Domingo
            TimeSpan? sundayMorningStart, TimeSpan? sundayMorningEnd,
            TimeSpan? sundayAfternoonStart, TimeSpan? sundayAfternoonEnd)
        {
            // Lunes
            MondayMorningStart = mondayMorningStart;
            MondayMorningEnd = mondayMorningEnd;
            MondayAfternoonStart = mondayAfternoonStart;
            MondayAfternoonEnd = mondayAfternoonEnd;
            
            // Martes
            TuesdayMorningStart = tuesdayMorningStart;
            TuesdayMorningEnd = tuesdayMorningEnd;
            TuesdayAfternoonStart = tuesdayAfternoonStart;
            TuesdayAfternoonEnd = tuesdayAfternoonEnd;
            
            // Miércoles
            WednesdayMorningStart = wednesdayMorningStart;
            WednesdayMorningEnd = wednesdayMorningEnd;
            WednesdayAfternoonStart = wednesdayAfternoonStart;
            WednesdayAfternoonEnd = wednesdayAfternoonEnd;
            
            // Jueves
            ThursdayMorningStart = thursdayMorningStart;
            ThursdayMorningEnd = thursdayMorningEnd;
            ThursdayAfternoonStart = thursdayAfternoonStart;
            ThursdayAfternoonEnd = thursdayAfternoonEnd;
            
            // Viernes
            FridayMorningStart = fridayMorningStart;
            FridayMorningEnd = fridayMorningEnd;
            FridayAfternoonStart = fridayAfternoonStart;
            FridayAfternoonEnd = fridayAfternoonEnd;
            
            // Sábado
            SaturdayMorningStart = saturdayMorningStart;
            SaturdayMorningEnd = saturdayMorningEnd;
            SaturdayAfternoonStart = saturdayAfternoonStart;
            SaturdayAfternoonEnd = saturdayAfternoonEnd;
            
            // Domingo
            SundayMorningStart = sundayMorningStart;
            SundayMorningEnd = sundayMorningEnd;
            SundayAfternoonStart = sundayAfternoonStart;
            SundayAfternoonEnd = sundayAfternoonEnd;
            
            UpdatedAt = DateTime.UtcNow;
        }
    }
}