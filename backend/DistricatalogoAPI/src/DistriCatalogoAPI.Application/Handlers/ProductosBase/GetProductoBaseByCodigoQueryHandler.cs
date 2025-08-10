using MediatR;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Application.Queries.ProductosBase;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.ProductosBase
{
    public class GetProductoBaseByCodigoQueryHandler : IRequestHandler<GetProductoBaseByCodigoQuery, GetProductoBaseByCodigoQueryResult?>
    {
        private readonly IProductBaseRepository _productRepository;
        private readonly ILogger<GetProductoBaseByCodigoQueryHandler> _logger;

        public GetProductoBaseByCodigoQueryHandler(
            IProductBaseRepository productRepository,
            ILogger<GetProductoBaseByCodigoQueryHandler> logger)
        {
            _productRepository = productRepository;
            _logger = logger;
        }

        public async Task<GetProductoBaseByCodigoQueryResult?> Handle(GetProductoBaseByCodigoQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Obteniendo producto base por código: {Codigo}", request.Codigo);

            var product = await _productRepository.GetByCodigoAsync(request.Codigo);
            
            if (product == null)
            {
                _logger.LogWarning("Producto no encontrado con código: {Codigo}", request.Codigo);
                return null;
            }

            return new GetProductoBaseByCodigoQueryResult
            {
                Id = product.Id,
                Codigo = product.Codigo,
                Descripcion = product.Descripcion,
                CodigoRubro = product.CodigoRubro,
                Precio = product.Precio,
                Existencia = product.Existencia,
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
                UpdatedAt = product.UpdatedAt
            };
        }
    }
}