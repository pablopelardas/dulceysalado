using System;
using MediatR;
using DistriCatalogoAPI.Application.DTOs;

namespace DistriCatalogoAPI.Application.Commands.Delivery
{
    public class CreateDeliveryScheduleCommand : IRequest<DeliveryScheduleDto>
    {
        public int DeliverySettingsId { get; set; }
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
}