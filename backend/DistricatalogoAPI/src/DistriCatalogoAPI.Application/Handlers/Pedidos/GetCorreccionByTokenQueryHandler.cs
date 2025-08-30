using MediatR;
using Microsoft.Extensions.Logging;
using AutoMapper;
using DistriCatalogoAPI.Application.Queries.Pedidos;
using DistriCatalogoAPI.Application.DTOs;
using DistriCatalogoAPI.Application.Helpers;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.Pedidos
{
    public class GetCorreccionByTokenQueryHandler : IRequestHandler<GetCorreccionByTokenQuery, CorreccionDto?>
    {
        private readonly ICorreccionTokenRepository _tokenRepository;
        private readonly IPedidoRepository _pedidoRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetCorreccionByTokenQueryHandler> _logger;

        public GetCorreccionByTokenQueryHandler(
            ICorreccionTokenRepository tokenRepository,
            IPedidoRepository pedidoRepository,
            IMapper mapper,
            ILogger<GetCorreccionByTokenQueryHandler> logger)
        {
            _tokenRepository = tokenRepository;
            _pedidoRepository = pedidoRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<CorreccionDto?> Handle(GetCorreccionByTokenQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var token = await _tokenRepository.GetByTokenAsync(request.Token);
                if (token == null)
                {
                    _logger.LogWarning("Token de corrección no encontrado: {Token}", request.Token);
                    return null;
                }

                var pedido = await _pedidoRepository.GetByIdWithItemsAsync(token.PedidoId);
                if (pedido == null)
                {
                    _logger.LogWarning("Pedido {PedidoId} no encontrado para token {Token}", 
                        token.PedidoId, request.Token);
                    return null;
                }

                // Obtener datos del pedido original desde el snapshot
                var pedidoOriginal = token.GetPedidoOriginal();
                
                // Obtener historial de correcciones del pedido
                var historialCorrecciones = await _tokenRepository.GetByPedidoIdAsync(pedido.Id);
                
                var correccionDto = new CorreccionDto
                {
                    Token = token.Token,
                    PedidoId = pedido.Id,
                    PedidoNumero = pedido.Numero,
                    FechaExpiracion = DateTimeHelper.ToArgentinaTime(token.FechaExpiracion),
                    EsValido = token.EsValido(),
                    ClienteNombre = pedido.Cliente?.Nombre ?? "Cliente",
                    ClienteEmail = pedido.Cliente?.Email ?? "",
                    
                    PedidoOriginal = new CorreccionDto.PedidoOriginalDto
                    {
                        Items = pedidoOriginal?.Items.Select(i => new CorreccionDto.ItemPedidoDto
                        {
                            CodigoProducto = i.CodigoProducto,
                            NombreProducto = i.NombreProducto,
                            Cantidad = i.Cantidad,
                            PrecioUnitario = i.PrecioUnitario,
                            Subtotal = i.Subtotal
                        }).ToList() ?? new List<CorreccionDto.ItemPedidoDto>(),
                        MontoTotal = pedidoOriginal?.MontoTotal ?? 0
                    },
                    
                    PedidoCorregido = new CorreccionDto.PedidoCorregidoDto
                    {
                        Items = pedido.Items.Select(i => new CorreccionDto.ItemPedidoDto
                        {
                            CodigoProducto = i.CodigoProducto,
                            NombreProducto = i.NombreProducto,
                            Cantidad = i.Cantidad,
                            PrecioUnitario = i.PrecioUnitario,
                            Subtotal = i.Subtotal,
                            Motivo = i.Observaciones // Las observaciones del item contienen el motivo específico
                        }).ToList(),
                        MontoTotal = pedido.MontoTotal,
                        MotivoCorreccion = token.MotivoCorreccion
                    },
                    
                    // Mapear historial de correcciones
                    HistorialCorrecciones = _mapper.Map<List<CorreccionTokenDto>>(historialCorrecciones)
                };

                return correccionDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo corrección por token {Token}", request.Token);
                return null;
            }
        }
    }
}