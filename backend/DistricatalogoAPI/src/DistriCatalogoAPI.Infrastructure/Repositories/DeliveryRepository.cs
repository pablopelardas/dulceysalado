using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Domain.Entities;
using DistriCatalogoAPI.Domain.Enums;
using DistriCatalogoAPI.Domain.Interfaces;
using DistriCatalogoAPI.Infrastructure.Models;

namespace DistriCatalogoAPI.Infrastructure.Repositories
{
    public class DeliveryRepository : IDeliveryRepository
    {
        private readonly DistricatalogoContext _context;
        private readonly ILogger<DeliveryRepository> _logger;

        public DeliveryRepository(DistricatalogoContext context, ILogger<DeliveryRepository> logger)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        #region DeliverySettings

        public async Task<DeliverySettings?> GetDeliverySettingsAsync(int empresaId)
        {
            _logger.LogDebug("Retrieving delivery settings for empresa: {EmpresaId}", empresaId);

            var settings = await _context.DeliverySettings
                .Include(ds => ds.Schedules)
                .Include(ds => ds.Slots)
                .FirstOrDefaultAsync(ds => ds.EmpresaId == empresaId);

            if (settings == null)
            {
                _logger.LogDebug("No delivery settings found for empresa: {EmpresaId}", empresaId);
                return null;
            }

            return MapDeliverySettingsToDomain(settings);
        }

        public async Task<DeliverySettings?> GetDeliverySettingsByIdAsync(int id)
        {
            _logger.LogDebug("Retrieving delivery settings by ID: {Id}", id);

            var settings = await _context.DeliverySettings
                .Include(ds => ds.Schedules)
                .Include(ds => ds.Slots)
                .FirstOrDefaultAsync(ds => ds.Id == id);

            if (settings == null)
            {
                _logger.LogDebug("No delivery settings found with ID: {Id}", id);
                return null;
            }

            return MapDeliverySettingsToDomain(settings);
        }

        public async Task<DeliverySettings> CreateDeliverySettingsAsync(DeliverySettings settings)
        {
            _logger.LogDebug("Creating delivery settings for empresa: {EmpresaId}", settings.EmpresaId);

            var model = MapDeliverySettingsToModel(settings);
            _context.DeliverySettings.Add(model);

            return settings;
        }

        public async Task UpdateDeliverySettingsAsync(DeliverySettings settings)
        {
            _logger.LogDebug("Updating delivery settings: {SettingsId}", settings.Id);

            var existing = await _context.DeliverySettings.FindAsync(settings.Id);
            if (existing == null)
            {
                throw new InvalidOperationException($"DeliverySettings with ID {settings.Id} not found");
            }

            UpdateDeliverySettingsModel(existing, settings);
        }

        #endregion

        #region DeliverySchedule

        public async Task<DeliverySchedule?> GetDeliveryScheduleAsync(int deliverySettingsId, DateOnly date)
        {
            var schedule = await _context.DeliverySchedules
                .Include(ds => ds.Slots)
                .FirstOrDefaultAsync(ds => ds.DeliverySettingsId == deliverySettingsId && ds.Date == date);

            return schedule != null ? MapDeliveryScheduleToDomain(schedule) : null;
        }

        public async Task<DeliverySchedule?> GetDeliveryScheduleByIdAsync(int id)
        {
            _logger.LogDebug("Retrieving delivery schedule by ID: {Id}", id);

            var schedule = await _context.DeliverySchedules
                .Include(ds => ds.Slots)
                .FirstOrDefaultAsync(ds => ds.Id == id);

            return schedule != null ? MapDeliveryScheduleToDomain(schedule) : null;
        }

        public async Task<IEnumerable<DeliverySchedule>> GetDeliverySchedulesAsync(int deliverySettingsId, DateOnly startDate, DateOnly endDate)
        {
            var schedules = await _context.DeliverySchedules
                .Include(ds => ds.Slots)
                .Where(ds => ds.DeliverySettingsId == deliverySettingsId && 
                           ds.Date >= startDate && ds.Date <= endDate)
                .ToListAsync();

            return schedules.Select(MapDeliveryScheduleToDomain);
        }

        public async Task<IEnumerable<DeliverySchedule>> GetSchedulesAsync(int deliverySettingsId, bool futureOnly = true)
        {
            _logger.LogDebug("Retrieving delivery schedules for settings: {DeliverySettingsId}, futureOnly: {FutureOnly}", 
                deliverySettingsId, futureOnly);

            var query = _context.DeliverySchedules
                .Include(ds => ds.Slots)
                .Where(ds => ds.DeliverySettingsId == deliverySettingsId);

            if (futureOnly)
            {
                var today = DateOnly.FromDateTime(DateTime.Today);
                query = query.Where(ds => ds.Date >= today);
            }

            var schedules = await query
                .OrderBy(ds => ds.Date)
                .ToListAsync();

            _logger.LogDebug("Found {Count} delivery schedules", schedules.Count);

            return schedules.Select(MapDeliveryScheduleToDomain);
        }

        public async Task<DeliverySchedule> CreateDeliveryScheduleAsync(DeliverySchedule schedule)
        {
            var model = MapDeliveryScheduleToModel(schedule);
            _context.DeliverySchedules.Add(model);
            return schedule;
        }

        public async Task UpdateDeliveryScheduleAsync(DeliverySchedule schedule)
        {
            var existing = await _context.DeliverySchedules.FindAsync(schedule.Id);
            if (existing == null)
            {
                throw new InvalidOperationException($"DeliverySchedule with ID {schedule.Id} not found");
            }

            UpdateDeliveryScheduleModel(existing, schedule);
        }

        public async Task DeleteDeliveryScheduleAsync(int id)
        {
            var schedule = await _context.DeliverySchedules.FindAsync(id);
            if (schedule != null)
            {
                _context.DeliverySchedules.Remove(schedule);
            }
        }

        #endregion

        #region DeliverySlots

        public async Task<DeliverySlot?> GetDeliverySlotAsync(int deliverySettingsId, DateOnly date, SlotType slotType)
        {
            var slot = await _context.DeliverySlots
                .FirstOrDefaultAsync(ds => ds.DeliverySettingsId == deliverySettingsId && 
                                         ds.Date == date && ds.SlotType == slotType);

            return slot != null ? MapDeliverySlotToDomain(slot) : null;
        }

        public async Task<IEnumerable<DeliverySlot>> GetDeliverySlotsAsync(int deliverySettingsId, DateOnly date)
        {
            var slots = await _context.DeliverySlots
                .Where(ds => ds.DeliverySettingsId == deliverySettingsId && ds.Date == date)
                .ToListAsync();

            return slots.Select(MapDeliverySlotToDomain);
        }

        public async Task<IEnumerable<DeliverySlot>> GetDeliverySlotsRangeAsync(int deliverySettingsId, DateOnly startDate, DateOnly endDate)
        {
            var slots = await _context.DeliverySlots
                .Where(ds => ds.DeliverySettingsId == deliverySettingsId && 
                           ds.Date >= startDate && ds.Date <= endDate)
                .ToListAsync();

            return slots.Select(MapDeliverySlotToDomain);
        }

        public async Task<DeliverySlot> CreateDeliverySlotAsync(DeliverySlot slot)
        {
            var model = MapDeliverySlotToModel(slot);
            _context.DeliverySlots.Add(model);
            await _context.SaveChangesAsync(); // Guardar para obtener el ID
            slot.SetId(model.Id); // Actualizar el ID en el objeto de dominio
            return slot;
        }

        public async Task UpdateDeliverySlotAsync(DeliverySlot slot)
        {
            var existing = await _context.DeliverySlots.FindAsync(slot.Id);
            if (existing == null)
            {
                throw new InvalidOperationException($"DeliverySlot with ID {slot.Id} not found");
            }

            UpdateDeliverySlotModel(existing, slot);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ReserveSlotAsync(int empresaId, DateOnly date, SlotType slotType)
        {
            _logger.LogInformation("ReserveSlotAsync: Attempting to reserve slot for empresa {EmpresaId}, date {Date}, slotType {SlotType}", 
                empresaId, date, slotType);

            var settings = await GetDeliverySettingsAsync(empresaId);
            if (settings == null)
            {
                throw new InvalidOperationException($"No delivery settings found for empresa {empresaId}");
            }

            var slot = await GetDeliverySlotAsync(settings.Id, date, slotType);
            
            if (slot == null)
            {
                _logger.LogInformation("ReserveSlotAsync: Creating new slot for date {Date}, slotType {SlotType}", date, slotType);
                // Crear nuevo slot si no existe
                slot = new DeliverySlot(settings.Id, date, slotType);
                await CreateDeliverySlotAsync(slot);
            }
            else
            {
                _logger.LogInformation("ReserveSlotAsync: Found existing slot with CurrentCapacity {CurrentCapacity}", slot.CurrentCapacity);
            }

            // Verificar si hay configuración específica para esta fecha
            var schedule = await GetDeliveryScheduleAsync(settings.Id, date);
            int maxCapacity;
            
            if (schedule != null)
            {
                _logger.LogInformation("ReserveSlotAsync: Found custom schedule for date {Date}", date);
                // Usar capacidad personalizada de la fecha especial si está definida
                maxCapacity = slotType == SlotType.Morning 
                    ? (schedule.CustomMaxCapacityMorning ?? settings.MaxCapacityMorning)
                    : (schedule.CustomMaxCapacityAfternoon ?? settings.MaxCapacityAfternoon);
                _logger.LogInformation("ReserveSlotAsync: Using custom maxCapacity {MaxCapacity} for {SlotType}", maxCapacity, slotType);
            }
            else
            {
                _logger.LogInformation("ReserveSlotAsync: No custom schedule found, using default settings");
                // Usar capacidad general de la configuración
                maxCapacity = slotType == SlotType.Morning ? settings.MaxCapacityMorning : settings.MaxCapacityAfternoon;
                _logger.LogInformation("ReserveSlotAsync: Using default maxCapacity {MaxCapacity} for {SlotType}", maxCapacity, slotType);
            }
            
            _logger.LogInformation("ReserveSlotAsync: Checking capacity - Current: {CurrentCapacity}, Max: {MaxCapacity}", 
                slot.CurrentCapacity, maxCapacity);

            if (slot.CurrentCapacity >= maxCapacity)
            {
                _logger.LogWarning("ReserveSlotAsync: Slot is full - Current: {CurrentCapacity} >= Max: {MaxCapacity}", 
                    slot.CurrentCapacity, maxCapacity);
                return false; // No hay capacidad disponible
            }

            slot.ReserveSlot();
            await UpdateDeliverySlotAsync(slot);
            
            _logger.LogInformation("ReserveSlotAsync: Slot reserved successfully. New CurrentCapacity: {CurrentCapacity}", 
                slot.CurrentCapacity);
            
            return true;
        }

        public async Task<bool> ReleaseSlotAsync(int empresaId, DateOnly date, SlotType slotType)
        {
            var settings = await GetDeliverySettingsAsync(empresaId);
            if (settings == null)
            {
                return false;
            }

            var slot = await GetDeliverySlotAsync(settings.Id, date, slotType);
            if (slot == null || slot.CurrentCapacity <= 0)
            {
                return false;
            }

            slot.ReleaseSlot();
            await UpdateDeliverySlotAsync(slot);
            
            return true;
        }

        #endregion

        #region General

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        #endregion

        #region Mapping Methods

        private DeliverySettings MapDeliverySettingsToDomain(Domain.Entities.DeliverySettings model)
        {
            var settings = new DeliverySettings(
                model.EmpresaId,
                model.MinSlotsAhead,
                model.MaxCapacityMorning,
                model.MaxCapacityAfternoon
            );

            settings.SetId(model.Id);
            settings.SetWeeklySchedule(
                model.MondayEnabled,
                model.TuesdayEnabled,
                model.WednesdayEnabled,
                model.ThursdayEnabled,
                model.FridayEnabled,
                model.SaturdayEnabled,
                model.SundayEnabled
            );

            // Mapear horarios por día
            settings.UpdateDaySchedules(
                // Lunes
                model.MondayMorningStart, model.MondayMorningEnd,
                model.MondayAfternoonStart, model.MondayAfternoonEnd,
                // Martes
                model.TuesdayMorningStart, model.TuesdayMorningEnd,
                model.TuesdayAfternoonStart, model.TuesdayAfternoonEnd,
                // Miércoles
                model.WednesdayMorningStart, model.WednesdayMorningEnd,
                model.WednesdayAfternoonStart, model.WednesdayAfternoonEnd,
                // Jueves
                model.ThursdayMorningStart, model.ThursdayMorningEnd,
                model.ThursdayAfternoonStart, model.ThursdayAfternoonEnd,
                // Viernes
                model.FridayMorningStart, model.FridayMorningEnd,
                model.FridayAfternoonStart, model.FridayAfternoonEnd,
                // Sábado
                model.SaturdayMorningStart, model.SaturdayMorningEnd,
                model.SaturdayAfternoonStart, model.SaturdayAfternoonEnd,
                // Domingo
                model.SundayMorningStart, model.SundayMorningEnd,
                model.SundayAfternoonStart, model.SundayAfternoonEnd
            );

            // Mapear schedules y slots si están incluidos
            if (model.Schedules?.Any() == true)
            {
                foreach (var schedule in model.Schedules)
                {
                    // Aquí podrías agregar la lógica para mapear schedules si es necesario
                }
            }

            return settings;
        }

        private Domain.Entities.DeliverySettings MapDeliverySettingsToModel(DeliverySettings domain)
        {
            return new Domain.Entities.DeliverySettings
            {
                Id = domain.Id,
                EmpresaId = domain.EmpresaId,
                MinSlotsAhead = domain.MinSlotsAhead,
                MaxCapacityMorning = domain.MaxCapacityMorning,
                MaxCapacityAfternoon = domain.MaxCapacityAfternoon,
                
                // Días habilitados
                MondayEnabled = domain.MondayEnabled,
                TuesdayEnabled = domain.TuesdayEnabled,
                WednesdayEnabled = domain.WednesdayEnabled,
                ThursdayEnabled = domain.ThursdayEnabled,
                FridayEnabled = domain.FridayEnabled,
                SaturdayEnabled = domain.SaturdayEnabled,
                SundayEnabled = domain.SundayEnabled,
                
                // Horarios por día - Lunes
                MondayMorningStart = domain.MondayMorningStart,
                MondayMorningEnd = domain.MondayMorningEnd,
                MondayAfternoonStart = domain.MondayAfternoonStart,
                MondayAfternoonEnd = domain.MondayAfternoonEnd,
                
                // Horarios por día - Martes
                TuesdayMorningStart = domain.TuesdayMorningStart,
                TuesdayMorningEnd = domain.TuesdayMorningEnd,
                TuesdayAfternoonStart = domain.TuesdayAfternoonStart,
                TuesdayAfternoonEnd = domain.TuesdayAfternoonEnd,
                
                // Horarios por día - Miércoles
                WednesdayMorningStart = domain.WednesdayMorningStart,
                WednesdayMorningEnd = domain.WednesdayMorningEnd,
                WednesdayAfternoonStart = domain.WednesdayAfternoonStart,
                WednesdayAfternoonEnd = domain.WednesdayAfternoonEnd,
                
                // Horarios por día - Jueves
                ThursdayMorningStart = domain.ThursdayMorningStart,
                ThursdayMorningEnd = domain.ThursdayMorningEnd,
                ThursdayAfternoonStart = domain.ThursdayAfternoonStart,
                ThursdayAfternoonEnd = domain.ThursdayAfternoonEnd,
                
                // Horarios por día - Viernes
                FridayMorningStart = domain.FridayMorningStart,
                FridayMorningEnd = domain.FridayMorningEnd,
                FridayAfternoonStart = domain.FridayAfternoonStart,
                FridayAfternoonEnd = domain.FridayAfternoonEnd,
                
                // Horarios por día - Sábado
                SaturdayMorningStart = domain.SaturdayMorningStart,
                SaturdayMorningEnd = domain.SaturdayMorningEnd,
                SaturdayAfternoonStart = domain.SaturdayAfternoonStart,
                SaturdayAfternoonEnd = domain.SaturdayAfternoonEnd,
                
                // Horarios por día - Domingo
                SundayMorningStart = domain.SundayMorningStart,
                SundayMorningEnd = domain.SundayMorningEnd,
                SundayAfternoonStart = domain.SundayAfternoonStart,
                SundayAfternoonEnd = domain.SundayAfternoonEnd,
                
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        private void UpdateDeliverySettingsModel(Domain.Entities.DeliverySettings model, DeliverySettings domain)
        {
            model.MinSlotsAhead = domain.MinSlotsAhead;
            model.MaxCapacityMorning = domain.MaxCapacityMorning;
            model.MaxCapacityAfternoon = domain.MaxCapacityAfternoon;
            
            // Días habilitados
            model.MondayEnabled = domain.MondayEnabled;
            model.TuesdayEnabled = domain.TuesdayEnabled;
            model.WednesdayEnabled = domain.WednesdayEnabled;
            model.ThursdayEnabled = domain.ThursdayEnabled;
            model.FridayEnabled = domain.FridayEnabled;
            model.SaturdayEnabled = domain.SaturdayEnabled;
            model.SundayEnabled = domain.SundayEnabled;
            
            // Horarios por día - Lunes
            model.MondayMorningStart = domain.MondayMorningStart;
            model.MondayMorningEnd = domain.MondayMorningEnd;
            model.MondayAfternoonStart = domain.MondayAfternoonStart;
            model.MondayAfternoonEnd = domain.MondayAfternoonEnd;
            
            // Horarios por día - Martes
            model.TuesdayMorningStart = domain.TuesdayMorningStart;
            model.TuesdayMorningEnd = domain.TuesdayMorningEnd;
            model.TuesdayAfternoonStart = domain.TuesdayAfternoonStart;
            model.TuesdayAfternoonEnd = domain.TuesdayAfternoonEnd;
            
            // Horarios por día - Miércoles
            model.WednesdayMorningStart = domain.WednesdayMorningStart;
            model.WednesdayMorningEnd = domain.WednesdayMorningEnd;
            model.WednesdayAfternoonStart = domain.WednesdayAfternoonStart;
            model.WednesdayAfternoonEnd = domain.WednesdayAfternoonEnd;
            
            // Horarios por día - Jueves
            model.ThursdayMorningStart = domain.ThursdayMorningStart;
            model.ThursdayMorningEnd = domain.ThursdayMorningEnd;
            model.ThursdayAfternoonStart = domain.ThursdayAfternoonStart;
            model.ThursdayAfternoonEnd = domain.ThursdayAfternoonEnd;
            
            // Horarios por día - Viernes
            model.FridayMorningStart = domain.FridayMorningStart;
            model.FridayMorningEnd = domain.FridayMorningEnd;
            model.FridayAfternoonStart = domain.FridayAfternoonStart;
            model.FridayAfternoonEnd = domain.FridayAfternoonEnd;
            
            // Horarios por día - Sábado
            model.SaturdayMorningStart = domain.SaturdayMorningStart;
            model.SaturdayMorningEnd = domain.SaturdayMorningEnd;
            model.SaturdayAfternoonStart = domain.SaturdayAfternoonStart;
            model.SaturdayAfternoonEnd = domain.SaturdayAfternoonEnd;
            
            // Horarios por día - Domingo
            model.SundayMorningStart = domain.SundayMorningStart;
            model.SundayMorningEnd = domain.SundayMorningEnd;
            model.SundayAfternoonStart = domain.SundayAfternoonStart;
            model.SundayAfternoonEnd = domain.SundayAfternoonEnd;
            
            model.UpdatedAt = DateTime.UtcNow;
        }

        private DeliverySchedule MapDeliveryScheduleToDomain(Domain.Entities.DeliverySchedule model)
        {
            var schedule = new DeliverySchedule(model.DeliverySettingsId, model.Date, model.MorningEnabled, model.AfternoonEnabled);
            schedule.SetId(model.Id);
            
            // Mapear capacidades customizadas
            schedule.CustomMaxCapacityMorning = model.CustomMaxCapacityMorning;
            schedule.CustomMaxCapacityAfternoon = model.CustomMaxCapacityAfternoon;
            
            // Mapear horarios customizados
            schedule.CustomMorningStartTime = model.CustomMorningStartTime;
            schedule.CustomMorningEndTime = model.CustomMorningEndTime;
            schedule.CustomAfternoonStartTime = model.CustomAfternoonStartTime;
            schedule.CustomAfternoonEndTime = model.CustomAfternoonEndTime;
            
            return schedule;
        }

        private Domain.Entities.DeliverySchedule MapDeliveryScheduleToModel(DeliverySchedule domain)
        {
            return new Domain.Entities.DeliverySchedule
            {
                Id = domain.Id,
                DeliverySettingsId = domain.DeliverySettingsId,
                Date = domain.Date,
                MorningEnabled = domain.MorningEnabled,
                AfternoonEnabled = domain.AfternoonEnabled,
                CustomMaxCapacityMorning = domain.CustomMaxCapacityMorning,
                CustomMaxCapacityAfternoon = domain.CustomMaxCapacityAfternoon,
                CustomMorningStartTime = domain.CustomMorningStartTime,
                CustomMorningEndTime = domain.CustomMorningEndTime,
                CustomAfternoonStartTime = domain.CustomAfternoonStartTime,
                CustomAfternoonEndTime = domain.CustomAfternoonEndTime,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        private void UpdateDeliveryScheduleModel(Domain.Entities.DeliverySchedule model, DeliverySchedule domain)
        {
            model.MorningEnabled = domain.MorningEnabled;
            model.AfternoonEnabled = domain.AfternoonEnabled;
            model.CustomMaxCapacityMorning = domain.CustomMaxCapacityMorning;
            model.CustomMaxCapacityAfternoon = domain.CustomMaxCapacityAfternoon;
            model.CustomMorningStartTime = domain.CustomMorningStartTime;
            model.CustomMorningEndTime = domain.CustomMorningEndTime;
            model.CustomAfternoonStartTime = domain.CustomAfternoonStartTime;
            model.CustomAfternoonEndTime = domain.CustomAfternoonEndTime;
            model.UpdatedAt = DateTime.UtcNow;
        }

        private DeliverySlot MapDeliverySlotToDomain(Domain.Entities.DeliverySlot model)
        {
            var slot = new DeliverySlot(model.DeliverySettingsId, model.Date, model.SlotType);
            slot.SetId(model.Id);
            slot.SetCurrentCapacity(model.CurrentCapacity);
            if (model.DeliveryScheduleId.HasValue)
            {
                slot.SetDeliveryScheduleId(model.DeliveryScheduleId.Value);
            }
            return slot;
        }

        private Domain.Entities.DeliverySlot MapDeliverySlotToModel(DeliverySlot domain)
        {
            return new Domain.Entities.DeliverySlot
            {
                Id = domain.Id,
                DeliverySettingsId = domain.DeliverySettingsId,
                DeliveryScheduleId = domain.DeliveryScheduleId,
                Date = domain.Date,
                SlotType = domain.SlotType,
                CurrentCapacity = domain.CurrentCapacity,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };
        }

        private void UpdateDeliverySlotModel(Domain.Entities.DeliverySlot model, DeliverySlot domain)
        {
            model.CurrentCapacity = domain.CurrentCapacity;
            model.UpdatedAt = DateTime.UtcNow;
        }

        #endregion
    }
}