using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using DistriCatalogoAPI.Application.Queries.Delivery;
using DistriCatalogoAPI.Application.DTOs;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.Delivery
{
    public class GetDeliverySettingsQueryHandler : IRequestHandler<GetDeliverySettingsQuery, DeliverySettingsDto?>
    {
        private readonly IDeliveryRepository _deliveryRepository;
        private readonly IMapper _mapper;

        public GetDeliverySettingsQueryHandler(IDeliveryRepository deliveryRepository, IMapper mapper)
        {
            _deliveryRepository = deliveryRepository;
            _mapper = mapper;
        }

        public async Task<DeliverySettingsDto?> Handle(GetDeliverySettingsQuery request, CancellationToken cancellationToken)
        {
            var settings = await _deliveryRepository.GetDeliverySettingsAsync(request.EmpresaId);
            
            return settings == null ? null : _mapper.Map<DeliverySettingsDto>(settings);
        }
    }
}