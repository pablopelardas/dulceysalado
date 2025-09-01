using MediatR;
using DistriCatalogoAPI.Application.DTOs;

namespace DistriCatalogoAPI.Application.Commands.Delivery
{
    public class UpdateDeliveryScheduleCommand : IRequest<DeliveryScheduleDto>
    {
        public int Id { get; set; }
        public bool MorningEnabled { get; set; }
        public bool AfternoonEnabled { get; set; }
        public int? CustomMaxCapacityMorning { get; set; }
        public int? CustomMaxCapacityAfternoon { get; set; }
        public TimeSpan? CustomMorningStartTime { get; set; }
        public TimeSpan? CustomMorningEndTime { get; set; }
        public TimeSpan? CustomAfternoonStartTime { get; set; }
        public TimeSpan? CustomAfternoonEndTime { get; set; }
    }
}