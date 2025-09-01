using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DistriCatalogoAPI.Domain.Entities;
using DistriCatalogoAPI.Domain.Enums;

namespace DistriCatalogoAPI.Domain.Interfaces
{
    public interface IDeliveryRepository
    {
        // DeliverySettings
        Task<DeliverySettings?> GetDeliverySettingsAsync(int empresaId);
        Task<DeliverySettings?> GetDeliverySettingsByIdAsync(int id);
        Task<DeliverySettings> CreateDeliverySettingsAsync(DeliverySettings settings);
        Task UpdateDeliverySettingsAsync(DeliverySettings settings);

        // DeliverySchedule
        Task<DeliverySchedule?> GetDeliveryScheduleAsync(int deliverySettingsId, DateOnly date);
        Task<DeliverySchedule?> GetDeliveryScheduleByIdAsync(int id);
        Task<IEnumerable<DeliverySchedule>> GetDeliverySchedulesAsync(int deliverySettingsId, DateOnly startDate, DateOnly endDate);
        Task<IEnumerable<DeliverySchedule>> GetSchedulesAsync(int deliverySettingsId, bool futureOnly = true);
        Task<DeliverySchedule> CreateDeliveryScheduleAsync(DeliverySchedule schedule);
        Task UpdateDeliveryScheduleAsync(DeliverySchedule schedule);
        Task DeleteDeliveryScheduleAsync(int id);

        // DeliverySlots
        Task<DeliverySlot?> GetDeliverySlotAsync(int deliverySettingsId, DateOnly date, SlotType slotType);
        Task<IEnumerable<DeliverySlot>> GetDeliverySlotsAsync(int deliverySettingsId, DateOnly date);
        Task<IEnumerable<DeliverySlot>> GetDeliverySlotsRangeAsync(int deliverySettingsId, DateOnly startDate, DateOnly endDate);
        Task<DeliverySlot> CreateDeliverySlotAsync(DeliverySlot slot);
        Task UpdateDeliverySlotAsync(DeliverySlot slot);
        Task<bool> ReserveSlotAsync(int empresaId, DateOnly date, SlotType slotType);
        Task<bool> ReleaseSlotAsync(int empresaId, DateOnly date, SlotType slotType);

        // General
        Task<int> SaveChangesAsync();
    }
}