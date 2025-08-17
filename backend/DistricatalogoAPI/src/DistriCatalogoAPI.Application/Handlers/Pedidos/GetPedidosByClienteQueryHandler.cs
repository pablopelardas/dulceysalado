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
    public class GetPedidosByClienteQueryHandler : IRequestHandler<GetPedidosByClienteQuery, PedidosPagedResultDto>
    {
        private readonly IPedidoRepository _pedidoRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetPedidosByClienteQueryHandler> _logger;

        public GetPedidosByClienteQueryHandler(
            IPedidoRepository pedidoRepository,
            IMapper mapper,
            ILogger<GetPedidosByClienteQueryHandler> logger)
        {
            _pedidoRepository = pedidoRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PedidosPagedResultDto> Handle(GetPedidosByClienteQuery request, CancellationToken cancellationToken)
        {
            var (pedidos, totalCount) = await _pedidoRepository.GetPaginatedByClienteAsync(
                request.ClienteId,
                request.EmpresaId,
                request.Page,
                request.PageSize,
                request.Estado,
                request.FechaDesde,
                request.FechaHasta);

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