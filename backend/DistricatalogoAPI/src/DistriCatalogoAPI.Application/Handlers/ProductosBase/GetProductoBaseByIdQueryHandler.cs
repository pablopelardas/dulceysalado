using MediatR;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Application.Queries.ProductosBase;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.ProductosBase
{
    public class GetProductoBaseByIdQueryHandler : IRequestHandler<GetProductoBaseByIdQuery, GetProductoBaseByIdQueryResult?>
    {
        private readonly IProductBaseRepository _productRepository;
        private readonly IProductBasePrecioRepository _precioRepository;
        private readonly IProductoBaseStockRepository _stockRepository;
        private readonly ILogger<GetProductoBaseByIdQueryHandler> _logger;

        public GetProductoBaseByIdQueryHandler(
            IProductBaseRepository productRepository,
            IProductBasePrecioRepository precioRepository,
            IProductoBaseStockRepository stockRepository,
            ILogger<GetProductoBaseByIdQueryHandler> logger)
        {
            _productRepository = productRepository;
            _precioRepository = precioRepository;
            _stockRepository = stockRepository;
            _logger = logger;
        }

        public async Task<GetProductoBaseByIdQueryResult?> Handle(GetProductoBaseByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Obteniendo producto base por ID: {Id}, EmpresaId: {EmpresaId}", request.Id, request.EmpresaId);

            var product = await _productRepository.GetByIdAsync(request.Id);
            
            if (product == null)
            {
                _logger.LogWarning("Producto no encontrado: {Id}", request.Id);
                return null;
            }

            // Obtener todos los precios del producto en todas las listas
            var precios = await _precioRepository.GetPreciosPorProductoAsync(product.Id);

            // Obtener stock específico de empresa o usar empresa principal (ID 1)
            var empresaIdObjetivo = request.EmpresaId ?? 1;
            var stockEmpresa = await _stockRepository.GetStockAsync(empresaIdObjetivo, product.Id);

            return new GetProductoBaseByIdQueryResult
            {
                Id = product.Id,
                Codigo = product.Codigo,
                Descripcion = product.Descripcion,
                CodigoRubro = product.CodigoRubro,
                Existencia = stockEmpresa, // Usar stock específico de empresa en lugar del stock base
                Visible = product.Visible,
                Destacado = product.Destacado,
                OrdenCategoria = product.OrdenCategoria,
                ImagenUrl = product.ImagenUrl,
                ImagenAlt = product.ImagenAlt,
                DescripcionCorta = product.DescripcionCorta,
                DescripcionLarga = product.DescripcionLarga,
                Tags = product.Tags,
                CodigoBarras = product.CodigoBarras,
                Marca = product.Marca,
                UnidadMedida = product.UnidadMedida,
                AdministradoPorEmpresaId = product.AdministradoPorEmpresaId,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt,
                Precios = precios.Select(p => new ProductoPrecioInfo
                {
                    ListaPrecioId = p.ListaPrecioId,
                    ListaPrecioCodigo = p.ListaPrecioCodigo,
                    ListaPrecioNombre = p.ListaPrecioNombre,
                    Precio = p.Precio
                }).ToList()
            };
        }
    }
}