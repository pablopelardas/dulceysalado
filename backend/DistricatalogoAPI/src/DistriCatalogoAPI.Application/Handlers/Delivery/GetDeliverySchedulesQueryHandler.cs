using MediatR;
using AutoMapper;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Application.Queries.Delivery;
using DistriCatalogoAPI.Application.DTOs;
using DistriCatalogoAPI.Domain.Interfaces;
using System.Linq;

namespace DistriCatalogoAPI.Application.Handlers.Delivery
{
    public class GetDeliverySchedulesQueryHandler : IRequestHandler<GetDeliverySchedulesQuery, List<DeliveryScheduleDto>>
    {
        private readonly IDeliveryRepository _deliveryRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetDeliverySchedulesQueryHandler> _logger;

        public GetDeliverySchedulesQueryHandler(
            IDeliveryRepository deliveryRepository,
            IMapper mapper,
            ILogger<GetDeliverySchedulesQueryHandler> logger)
        {
            _deliveryRepository = deliveryRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<List<DeliveryScheduleDto>> Handle(GetDeliverySchedulesQuery request, CancellationToken cancellationToken)
        {
            _logger.LogDebug("Getting delivery schedules for settings {DeliverySettingsId}, futureOnly: {FutureOnly}",
                request.DeliverySettingsId, request.FutureOnly);

            var schedules = await _deliveryRepository.GetSchedulesAsync(request.DeliverySettingsId, request.FutureOnly);
            var schedulesList = schedules.ToList();
            
            _logger.LogDebug("Found {Count} delivery schedules", schedulesList.Count);
            
            return _mapper.Map<List<DeliveryScheduleDto>>(schedulesList);
        }
    }
}