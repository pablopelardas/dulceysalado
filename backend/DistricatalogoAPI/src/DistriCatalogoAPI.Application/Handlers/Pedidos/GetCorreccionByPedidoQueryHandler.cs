using MediatR;
using Microsoft.Extensions.Logging;
using AutoMapper;
using DistriCatalogoAPI.Application.Queries.Pedidos;
using DistriCatalogoAPI.Application.DTOs;
using DistriCatalogoAPI.Application.Helpers;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.Pedidos
{
    public class GetCorreccionByPedidoQueryHandler : IRequestHandler<GetCorreccionByPedidoQuery, CorreccionDto?>
    {
        private readonly ICorreccionTokenRepository _tokenRepository;
        private readonly IPedidoRepository _pedidoRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<GetCorreccionByPedidoQueryHandler> _logger;

        public GetCorreccionByPedidoQueryHandler(
            ICorreccionTokenRepository tokenRepository,
            IPedidoRepository pedidoRepository,
            IMapper mapper,
            ILogger<GetCorreccionByPedidoQueryHandler> logger)
        {
            _tokenRepository = tokenRepository;
            _pedidoRepository = pedidoRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<CorreccionDto?> Handle(GetCorreccionByPedidoQuery request, CancellationToken cancellationToken)
        {
            try
            {
                // Verificar que el pedido existe y pertenece al cliente
                var pedido = await _pedidoRepository.GetByIdWithItemsAsync(request.PedidoId);
                if (pedido == null || pedido.ClienteId != request.ClienteId || pedido.EmpresaId != request.EmpresaId)
                {
                    _logger.LogWarning("Pedido {PedidoId} no encontrado o no pertenece al cliente {ClienteId}", 
                        request.PedidoId, request.ClienteId);
                    return null;
                }

                // Obtener el token de corrección más reciente para este pedido
                var tokens = await _tokenRepository.GetByPedidoIdAsync(request.PedidoId);
                var token = tokens.OrderByDescending(t => t.FechaCreacion).FirstOrDefault();
                if (token == null)
                {
                    _logger.LogWarning("No hay token de corrección para el pedido {PedidoId}", request.PedidoId);
                    return null;
                }

                // Obtener datos del pedido original desde el snapshot
                var pedidoOriginal = token.GetPedidoOriginal();
                
                // Obtener historial completo de correcciones del pedido
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
                            Motivo = i.Observaciones // Incluir las observaciones como motivo
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
                _logger.LogError(ex, "Error obteniendo corrección para pedido {PedidoId}", request.PedidoId);
                return null;
            }
        }
    }
}