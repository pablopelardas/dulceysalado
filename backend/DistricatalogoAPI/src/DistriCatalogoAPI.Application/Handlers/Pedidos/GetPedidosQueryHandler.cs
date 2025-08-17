using System.Threading;
using System.Threading.Tasks;
using AutoMapper;
using MediatR;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Application.DTOs;
using DistriCatalogoAPI.Application.Queries.Pedidos;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.Pedidos
{
    public class GetPedidosQueryHandler : IRequestHandler<GetPedidosQuery, PedidosPagedResultDto>
    {
        private readonly IPedidoRepository _pedidoRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetPedidosQueryHandler> _logger;

        public GetPedidosQueryHandler(
            IPedidoRepository pedidoRepository,
            IMapper mapper,
            ILogger<GetPedidosQueryHandler> logger)
        {
            _pedidoRepository = pedidoRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PedidosPagedResultDto> Handle(GetPedidosQuery request, CancellationToken cancellationToken)
        {
            var (pedidos, totalCount) = await _pedidoRepository.GetPaginatedAsync(
                request.EmpresaId,
                request.Page,
                request.PageSize,
                request.Estado,
                request.FechaDesde,
                request.FechaHasta,
                request.ClienteId,
                request.NumeroContiene);

            var pedidosDto = _mapper.Map<List<PedidoDto>>(pedidos);

            return new PedidosPagedResultDto
            {
                Items = pedidosDto,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize
            };
        }
    }
}