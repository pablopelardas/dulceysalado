namespace DistriCatalogoAPI.Domain.Entities
{
    public class DeliverySchedule
    {
        public int Id { get; set; }
        
        public int DeliverySettingsId { get; set; }
        
        public DateOnly Date { get; set; }
        
        public bool MorningEnabled { get; set; } = true;
        public bool AfternoonEnabled { get; set; } = true;
        
        // Capacidades personalizadas para esta fecha específica
        public int? CustomMaxCapacityMorning { get; set; }
        public int? CustomMaxCapacityAfternoon { get; set; }
        
        // Horarios personalizados para esta fecha específica
        public TimeSpan? CustomMorningStartTime { get; set; }
        public TimeSpan? CustomMorningEndTime { get; set; }
        public TimeSpan? CustomAfternoonStartTime { get; set; }
        public TimeSpan? CustomAfternoonEndTime { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        public virtual DeliverySettings DeliverySettings { get; set; } = null!;
        public virtual ICollection<DeliverySlot> Slots { get; set; } = new List<DeliverySlot>();

        // Constructor
        public DeliverySchedule() { }

        public DeliverySchedule(int deliverySettingsId, DateOnly date, bool morningEnabled, bool afternoonEnabled)
        {
            DeliverySettingsId = deliverySettingsId;
            Date = date;
            MorningEnabled = morningEnabled;
            AfternoonEnabled = afternoonEnabled;
        }

        public DeliverySchedule(int deliverySettingsId, DateOnly date, bool morningEnabled, bool afternoonEnabled,
            int? customMaxCapacityMorning, int? customMaxCapacityAfternoon,
            TimeSpan? customMorningStartTime, TimeSpan? customMorningEndTime,
            TimeSpan? customAfternoonStartTime, TimeSpan? customAfternoonEndTime)
        {
            DeliverySettingsId = deliverySettingsId;
            Date = date;
            MorningEnabled = morningEnabled;
            AfternoonEnabled = afternoonEnabled;
            CustomMaxCapacityMorning = customMaxCapacityMorning;
            CustomMaxCapacityAfternoon = customMaxCapacityAfternoon;
            CustomMorningStartTime = customMorningStartTime;
            CustomMorningEndTime = customMorningEndTime;
            CustomAfternoonStartTime = customAfternoonStartTime;
            CustomAfternoonEndTime = customAfternoonEndTime;
        }

        // Métodos de negocio
        public void SetId(int id)
        {
            Id = id;
        }
    }
}