using MediatR;
using DistriCatalogoAPI.Application.DTOs;

namespace DistriCatalogoAPI.Application.Queries.Delivery
{
    public class GetDeliverySettingsQuery : IRequest<DeliverySettingsDto?>
    {
        public int EmpresaId { get; set; }
    }
}