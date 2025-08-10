using System.Linq;
using MediatR;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Application.Queries.Categories;
using DistriCatalogoAPI.Domain.Entities;
using DistriCatalogoAPI.Domain.Interfaces;
using DistriCatalogoAPI.Application.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.Categories
{
    public class GetCategoriesBaseQueryHandler : IRequestHandler<GetCategoriesBaseQuery, GetCategoriesBaseQueryResult>
    {
        private readonly ICategoryBaseRepository _categoryRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetCategoriesBaseQueryHandler> _logger;

        public GetCategoriesBaseQueryHandler(
            ICategoryBaseRepository categoryRepository,
            ICurrentUserService currentUserService,
            ILogger<GetCategoriesBaseQueryHandler> logger)
        {
            _categoryRepository = categoryRepository;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<GetCategoriesBaseQueryResult> Handle(GetCategoriesBaseQuery request, CancellationToken cancellationToken)
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync();

            _logger.LogInformation("Obteniendo categorías base - Usuario: {UserId}, VisibleOnly: {VisibleOnly}", 
                currentUser.Id, request.VisibleOnly);

            List<CategoryBase> categories;

            if (request.EmpresaId.HasValue)
            {
                categories = await _categoryRepository.GetByEmpresaAsync(request.EmpresaId.Value);
            }
            else
            {
                // Por ahora obtenemos por empresa del usuario actual
                categories = await _categoryRepository.GetByEmpresaAsync(currentUser.CompanyId);
            }

            // Filtrar por visibilidad si es necesario
            if (request.VisibleOnly == true)
            {
                categories = categories.Where(c => c.Visible).ToList();
            }

            var categoryDtos = new List<CategoryBaseDto>();

            foreach (var category in categories)
            {
                var productCount = await _categoryRepository.GetProductCountByCategoryAsync(category.CodigoRubro);
                
                categoryDtos.Add(new CategoryBaseDto
                {
                    Id = category.Id,
                    CodigoRubro = category.CodigoRubro,
                    Nombre = category.Nombre,
                    Icono = category.Icono,
                    Visible = category.Visible,
                    Orden = category.Orden,
                    Color = category.Color,
                    Descripcion = category.Descripcion,
                    CreatedByEmpresaId = category.CreatedByEmpresaId,
                    ProductCount = productCount, // Conteo real de productos por categoría
                    CreatedAt = category.CreatedAt,
                    UpdatedAt = category.UpdatedAt
                });
            }

            // Ordenar por orden y luego por nombre
            categoryDtos = categoryDtos.OrderBy(c => c.Orden).ThenBy(c => c.Nombre).ToList();

            _logger.LogInformation("Se encontraron {Count} categorías base", categoryDtos.Count);

            return new GetCategoriesBaseQueryResult
            {
                Categories = categoryDtos
            };
        }
    }
}