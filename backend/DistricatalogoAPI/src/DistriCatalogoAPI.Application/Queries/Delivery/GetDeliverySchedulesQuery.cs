using MediatR;
using DistriCatalogoAPI.Application.DTOs;

namespace DistriCatalogoAPI.Application.Queries.Delivery
{
    public class GetDeliverySchedulesQuery : IRequest<List<DeliveryScheduleDto>>
    {
        public int DeliverySettingsId { get; set; }
        public bool FutureOnly { get; set; } = true;
    }
}