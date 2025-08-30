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
        private readonly ICorreccionTokenRepository _correccionTokenRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetPedidoByIdQueryHandler> _logger;

        public GetPedidoByIdQueryHandler(
            IPedidoRepository pedidoRepository,
            ICorreccionTokenRepository correccionTokenRepository,
            IMapper mapper,
            ILogger<GetPedidoByIdQueryHandler> logger)
        {
            _pedidoRepository = pedidoRepository;
            _correccionTokenRepository = correccionTokenRepository;
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

            var pedidoDto = _mapper.Map<PedidoDto>(pedido);
            
            // Incluir correcciones si se solicita
            if (request.IncludeCorrecciones)
            {
                var correcciones = await _correccionTokenRepository.GetByPedidoIdAsync(request.PedidoId);
                pedidoDto.Correcciones = _mapper.Map<List<CorreccionTokenDto>>(correcciones);
            }

            return pedidoDto;
        }
    }
}