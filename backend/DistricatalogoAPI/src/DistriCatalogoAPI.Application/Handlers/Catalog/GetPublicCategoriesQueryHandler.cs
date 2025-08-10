using MediatR;
using DistriCatalogoAPI.Application.Queries.Catalog;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.Catalog
{
    public class GetPublicCategoriesQueryHandler : IRequestHandler<GetPublicCategoriesQuery, GetPublicCategoriesQueryResult>
    {
        private readonly ICategoryEmpresaRepository _categoryRepository;
        private readonly ICatalogRepository _catalogRepository;
        private readonly ICompanyRepository _companyRepository;

        public GetPublicCategoriesQueryHandler(
            ICategoryEmpresaRepository categoryRepository,
            ICatalogRepository catalogRepository,
            ICompanyRepository companyRepository)
        {
            _categoryRepository = categoryRepository;
            _catalogRepository = catalogRepository;
            _companyRepository = companyRepository;
        }

        public async Task<GetPublicCategoriesQueryResult> Handle(GetPublicCategoriesQuery request, CancellationToken cancellationToken)
        {
            // Validar que la empresa existe
            var empresa = await _companyRepository.GetByIdAsync(request.EmpresaId);
            if (empresa == null)
            {
                return new GetPublicCategoriesQueryResult
                {
                    Categorias = new List<CategoriaPublicaDto>()
                };
            }
            // Obtener las categorías con conteo de productos real
            var categoriesWithCount = await _catalogRepository.GetCategoriesWithProductCountAsync(request.EmpresaId);
            
            var categoriasDtos = categoriesWithCount.Select(cat => new CategoriaPublicaDto
            {
                Id = cat.CategoryId,
                CodigoRubro = cat.CodigoRubro,
                Nombre = cat.Nombre,
                Descripcion = cat.Descripcion,
                Icono = cat.Icono,
                Color = cat.Color,
                Orden = cat.Orden,
                ProductCount = cat.ProductCount
            }).ToList();

            return new GetPublicCategoriesQueryResult
            {
                Categorias = categoriasDtos
            };
        }
    }
}