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
    public class GetPedidoByIdQueryHandler : IRequestHandler<GetPedidoByIdQuery, PedidoDto?>
    {
        private readonly IPedidoRepository _pedidoRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetPedidoByIdQueryHandler> _logger;

        public GetPedidoByIdQueryHandler(
            IPedidoRepository pedidoRepository,
            IMapper mapper,
            ILogger<GetPedidoByIdQueryHandler> logger)
        {
            _pedidoRepository = pedidoRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<PedidoDto?> Handle(GetPedidoByIdQuery request, CancellationToken cancellationToken)
        {
            var pedido = await _pedidoRepository.GetByIdAsync(request.PedidoId, request.IncludeItems);
            
            if (pedido == null || pedido.EmpresaId != request.EmpresaId)
            {
                return null;
            }

            return _mapper.Map<PedidoDto>(pedido);
        }
    }
}