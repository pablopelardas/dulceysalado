using MediatR;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Application.Queries.ProductosEmpresaPrecios;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.ProductosEmpresaPrecios
{
    public class GetPreciosPorProductoEmpresaQueryHandler : IRequestHandler<GetPreciosPorProductoEmpresaQuery, GetPreciosPorProductoEmpresaQueryResult>
    {
        private readonly IProductoEmpresaPrecioRepository _precioRepository;
        private readonly ILogger<GetPreciosPorProductoEmpresaQueryHandler> _logger;

        public GetPreciosPorProductoEmpresaQueryHandler(
            IProductoEmpresaPrecioRepository precioRepository,
            ILogger<GetPreciosPorProductoEmpresaQueryHandler> logger)
        {
            _precioRepository = precioRepository;
            _logger = logger;
        }

        public async Task<GetPreciosPorProductoEmpresaQueryResult> Handle(GetPreciosPorProductoEmpresaQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Obteniendo precios para producto empresa: {ProductoEmpresaId}", request.ProductoEmpresaId);

            var precios = await _precioRepository.GetPreciosPorProductoAsync(request.ProductoEmpresaId);

            var preciosDto = precios.Select(p => new PrecioEmpresaListaDto
            {
                ListaPrecioId = p.ListaPrecioId,
                ListaPrecioCodigo = p.ListaPrecioCodigo,
                ListaPrecioNombre = p.ListaPrecioNombre,
                Precio = p.Precio
            }).ToList();

            return new GetPreciosPorProductoEmpresaQueryResult
            {
                ProductoEmpresaId = request.ProductoEmpresaId,
                Precios = preciosDto
            };
        }
    }
}