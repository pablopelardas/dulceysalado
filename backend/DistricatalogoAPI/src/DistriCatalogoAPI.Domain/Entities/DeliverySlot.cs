using DistriCatalogoAPI.Domain.Enums;

namespace DistriCatalogoAPI.Domain.Entities
{
    public class DeliverySlot
    {
        public int Id { get; set; }
        
        public int DeliverySettingsId { get; set; }
        
        public int? DeliveryScheduleId { get; set; }
        
        public DateOnly Date { get; set; }
        
        public SlotType SlotType { get; set; }
        
        public int CurrentCapacity { get; set; } = 0;
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        
        public virtual DeliverySettings DeliverySettings { get; set; } = null!;
        public virtual DeliverySchedule? DeliverySchedule { get; set; }

        // Constructor
        public DeliverySlot() { }

        public DeliverySlot(int deliverySettingsId, DateOnly date, SlotType slotType)
        {
            DeliverySettingsId = deliverySettingsId;
            Date = date;
            SlotType = slotType;
            CurrentCapacity = 0;
        }

        // Métodos de negocio
        public void SetId(int id)
        {
            Id = id;
        }

        public void SetDeliveryScheduleId(int deliveryScheduleId)
        {
            DeliveryScheduleId = deliveryScheduleId;
        }

        public void SetCurrentCapacity(int capacity)
        {
            CurrentCapacity = capacity;
        }

        public bool IsAvailable(int maxCapacity)
        {
            return CurrentCapacity < maxCapacity;
        }
        
        public void ReserveSlot()
        {
            CurrentCapacity++;
            UpdatedAt = DateTime.UtcNow;
        }
        
        public void ReleaseSlot()
        {
            if (CurrentCapacity > 0)
            {
                CurrentCapacity--;
                UpdatedAt = DateTime.UtcNow;
            }
        }
    }
}