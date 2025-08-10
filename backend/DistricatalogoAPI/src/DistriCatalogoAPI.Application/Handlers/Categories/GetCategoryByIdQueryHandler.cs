using MediatR;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Application.Queries.Categories;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.Categories
{
    public class GetCategoryByIdQueryHandler : IRequestHandler<GetCategoryByIdQuery, GetCategoryByIdQueryResult?>
    {
        private readonly ICategoryBaseRepository _categoryBaseRepository;
        private readonly ICategoryEmpresaRepository _categoryEmpresaRepository;
        private readonly ILogger<GetCategoryByIdQueryHandler> _logger;

        public GetCategoryByIdQueryHandler(
            ICategoryBaseRepository categoryBaseRepository,
            ICategoryEmpresaRepository categoryEmpresaRepository,
            ILogger<GetCategoryByIdQueryHandler> logger)
        {
            _categoryBaseRepository = categoryBaseRepository;
            _categoryEmpresaRepository = categoryEmpresaRepository;
            _logger = logger;
        }

        public async Task<GetCategoryByIdQueryResult?> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Obteniendo categoría por ID: {Id}, EsBase: {IsBase}", request.Id, request.IsBaseCategory);

            if (request.IsBaseCategory)
            {
                var categoryBase = await _categoryBaseRepository.GetByIdAsync(request.Id);
                if (categoryBase == null)
                    return null;

                var productCount = await _categoryBaseRepository.GetProductCountByCategoryAsync(categoryBase.CodigoRubro);

                return new GetCategoryByIdQueryResult
                {
                    Id = categoryBase.Id,
                    EmpresaId = null,
                    CodigoRubro = categoryBase.CodigoRubro,
                    Nombre = categoryBase.Nombre,
                    Icono = categoryBase.Icono,
                    Visible = categoryBase.Visible,
                    Orden = categoryBase.Orden,
                    Color = categoryBase.Color,
                    Descripcion = categoryBase.Descripcion,
                    CreatedByEmpresaId = categoryBase.CreatedByEmpresaId,
                    ProductCount = productCount, // Conteo real de productos base por categoría
                    IsBaseCategory = true,
                    CreatedAt = categoryBase.CreatedAt,
                    UpdatedAt = categoryBase.UpdatedAt
                };
            }
            else
            {
                var categoryEmpresa = await _categoryEmpresaRepository.GetByIdAsync(request.Id);
                if (categoryEmpresa == null)
                    return null;

                var productCount = await _categoryEmpresaRepository.CountProductsAsync(categoryEmpresa.Id);

                return new GetCategoryByIdQueryResult
                {
                    Id = categoryEmpresa.Id,
                    EmpresaId = categoryEmpresa.EmpresaId,
                    CodigoRubro = categoryEmpresa.CodigoRubro,
                    Nombre = categoryEmpresa.Nombre,
                    Icono = categoryEmpresa.Icono,
                    Visible = categoryEmpresa.Visible,
                    Orden = categoryEmpresa.Orden,
                    Color = categoryEmpresa.Color,
                    Descripcion = categoryEmpresa.Descripcion,
                    CreatedByEmpresaId = null,
                    ProductCount = productCount,
                    IsBaseCategory = false,
                    CreatedAt = categoryEmpresa.CreatedAt,
                    UpdatedAt = categoryEmpresa.UpdatedAt
                };
            }
        }
    }
}