using MediatR;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Application.Queries.ProductosPrecios;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.ProductosPrecios
{
    public class GetPreciosPorProductoQueryHandler : IRequestHandler<GetPreciosPorProductoQuery, GetPreciosPorProductoQueryResult>
    {
        private readonly IProductBasePrecioRepository _precioRepository;
        private readonly ILogger<GetPreciosPorProductoQueryHandler> _logger;

        public GetPreciosPorProductoQueryHandler(
            IProductBasePrecioRepository precioRepository,
            ILogger<GetPreciosPorProductoQueryHandler> logger)
        {
            _precioRepository = precioRepository;
            _logger = logger;
        }

        public async Task<GetPreciosPorProductoQueryResult> Handle(GetPreciosPorProductoQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Obteniendo precios para producto: {ProductoId}", request.ProductoId);

            var precios = await _precioRepository.GetPreciosPorProductoAsync(request.ProductoId);

            var preciosDto = precios.Select(p => new PrecioListaDto
            {
                ListaPrecioId = p.ListaPrecioId,
                ListaPrecioCodigo = p.ListaPrecioCodigo,
                ListaPrecioNombre = p.ListaPrecioNombre,
                Precio = p.Precio
            }).ToList();

            return new GetPreciosPorProductoQueryResult
            {
                ProductoId = request.ProductoId,
                Precios = preciosDto
            };
        }
    }
}