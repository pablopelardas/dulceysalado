using MediatR;
using DistriCatalogoAPI.Application.Queries.Catalog;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.Catalog
{
    public class SearchPublicCatalogQueryHandler : IRequestHandler<SearchPublicCatalogQuery, SearchPublicCatalogQueryResult>
    {
        private readonly ICatalogRepository _catalogRepository;
        private readonly ICompanyRepository _companyRepository;

        public SearchPublicCatalogQueryHandler(
            ICatalogRepository catalogRepository,
            ICompanyRepository companyRepository)
        {
            _catalogRepository = catalogRepository;
            _companyRepository = companyRepository;
        }

        public async Task<SearchPublicCatalogQueryResult> Handle(SearchPublicCatalogQuery request, CancellationToken cancellationToken)
        {
            // Validar que la empresa existe
            var empresa = await _companyRepository.GetByIdAsync(request.EmpresaId);
            if (empresa == null)
            {
                return new SearchPublicCatalogQueryResult
                {
                    Productos = new List<ProductoCatalogoDto>(),
                    TotalCount = 0,
                    Page = 1,
                    PageSize = request.PageSize,
                    TotalPages = 0,
                    FiltrosDisponibles = new List<FiltroDisponibleDto>()
                };
            }
            var (products, totalCount) = await _catalogRepository.SearchCatalogAsync(
                request.EmpresaId,
                request.Texto,
                request.CodigosRubro,
                request.PrecioMinimo,
                request.PrecioMaximo,
                request.SoloDestacados,
                request.Tags,
                request.Marca,
                request.OrderBy,
                request.Ascending,
                request.Page,
                request.PageSize);

            // Convertir a DTOs
            var productos = products.Select(product => new ProductoCatalogoDto
            {
                Codigo = product.Codigo.ToString(),
                Nombre = product.Descripcion,
                Descripcion = product.DescripcionCorta ?? product.Descripcion,
                DescripcionCorta = product.DescripcionCorta,
                Precio = product.Precio,
                Destacado = product.Destacado,
                ImagenUrls = string.IsNullOrEmpty(product.ImagenUrl) ? new List<string>() : new List<string> { product.ImagenUrl },
                Tags = product.Tags?.ToList() ?? new List<string>(),
                TipoProducto = product.TipoProducto ?? "base",
                Marca = product.Marca,
                Unidad = product.UnidadMedida,
                CodigoBarras = product.CodigoBarras,
                CodigoRubro = product.CodigoRubro,
                ImagenAlt = product.ImagenAlt,
                Stock = (int?)product.Existencia
            }).ToList();

            // Generar filtros disponibles (simplificado)
            var filtrosDisponibles = new List<FiltroDisponibleDto>
            {
                new FiltroDisponibleDto
                {
                    Tipo = "destacado",
                    Nombre = "Productos Destacados",
                    Count = products.Count(p => p.Destacado)
                }
            };

            return new SearchPublicCatalogQueryResult
            {
                Productos = productos,
                TotalCount = totalCount,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize),
                FiltrosDisponibles = filtrosDisponibles
            };
        }
    }
}