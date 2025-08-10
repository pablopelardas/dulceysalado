using MediatR;
using DistriCatalogoAPI.Application.Queries.Catalog;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.Catalog
{
    public class GetFeaturedProductsQueryHandler : IRequestHandler<GetFeaturedProductsQuery, GetFeaturedProductsQueryResult>
    {
        private readonly ICatalogRepository _catalogRepository;
        private readonly ICompanyRepository _companyRepository;

        public GetFeaturedProductsQueryHandler(
            ICatalogRepository catalogRepository,
            ICompanyRepository companyRepository)
        {
            _catalogRepository = catalogRepository;
            _companyRepository = companyRepository;
        }

        public async Task<GetFeaturedProductsQueryResult> Handle(GetFeaturedProductsQuery request, CancellationToken cancellationToken)
        {
            // Obtener configuración de la empresa
            var empresa = await _companyRepository.GetByIdAsync(request.EmpresaId);
            if (empresa == null)
            {
                throw new KeyNotFoundException($"Empresa {request.EmpresaId} no encontrada");
            }

            var featuredProducts = await _catalogRepository.GetFeaturedProductsAsync(request.EmpresaId, request.ListaPrecioCodigo, request.Limit);

            var productos = featuredProducts.Select(p => new ProductoCatalogoDto
            {
                Codigo = p.Codigo.ToString(),
                Nombre = p.Descripcion,
                Descripcion = p.DescripcionCorta ?? p.Descripcion,
                DescripcionCorta = p.DescripcionCorta,
                Precio = empresa.MostrarPrecios ? p.Precio : null,
                Destacado = p.Destacado,
                ImagenUrls = string.IsNullOrEmpty(p.ImagenUrl) ? new List<string>() : new List<string> { p.ImagenUrl },
                Stock = empresa.MostrarStock ? (int?)p.Existencia : null,
                Tags = p.Tags?.ToList() ?? new List<string>(),
                Marca = p.Marca,
                Unidad = p.UnidadMedida,
                CodigoBarras = p.CodigoBarras,
                CodigoRubro = p.CodigoRubro,
                ImagenAlt = p.ImagenAlt,
                TipoProducto = p.TipoProducto ?? "base",
                
                // Price list information (solo si se muestran precios)
                ListaPrecioId = empresa.MostrarPrecios ? p.ListaPrecioId : null,
                ListaPrecioNombre = empresa.MostrarPrecios ? p.ListaPrecioNombre : null,
                ListaPrecioCodigo = empresa.MostrarPrecios ? p.ListaPrecioCodigo : null
            }).ToList();

            return new GetFeaturedProductsQueryResult
            {
                Productos = productos,
                TotalCount = productos.Count
            };
        }
    }
}