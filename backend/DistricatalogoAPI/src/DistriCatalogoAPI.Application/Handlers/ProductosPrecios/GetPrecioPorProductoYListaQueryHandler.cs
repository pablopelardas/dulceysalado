using MediatR;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Application.Queries.ProductosPrecios;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.ProductosPrecios
{
    public class GetPrecioPorProductoYListaQueryHandler : IRequestHandler<GetPrecioPorProductoYListaQuery, GetPrecioPorProductoYListaQueryResult?>
    {
        private readonly IProductBasePrecioRepository _precioRepository;
        private readonly ILogger<GetPrecioPorProductoYListaQueryHandler> _logger;

        public GetPrecioPorProductoYListaQueryHandler(
            IProductBasePrecioRepository precioRepository,
            ILogger<GetPrecioPorProductoYListaQueryHandler> logger)
        {
            _precioRepository = precioRepository;
            _logger = logger;
        }

        public async Task<GetPrecioPorProductoYListaQueryResult?> Handle(GetPrecioPorProductoYListaQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Obteniendo precio para producto {ProductoId} en lista {ListaId}", 
                request.ProductoId, request.ListaPrecioId);

            var precios = await _precioRepository.GetPreciosPorProductoYListaAsync(request.ProductoId, request.ListaPrecioId);
            var precio = precios.FirstOrDefault();

            if (precio == null)
            {
                _logger.LogWarning("Precio no encontrado para producto {ProductoId} en lista {ListaId}", 
                    request.ProductoId, request.ListaPrecioId);
                return null;
            }

            return new GetPrecioPorProductoYListaQueryResult
            {
                ProductoId = precio.ProductoBaseId,
                ListaPrecioId = precio.ListaPrecioId,
                ListaPrecioCodigo = precio.ListaPrecioCodigo,
                ListaPrecioNombre = precio.ListaPrecioNombre,
                Precio = precio.Precio
            };
        }
    }
}