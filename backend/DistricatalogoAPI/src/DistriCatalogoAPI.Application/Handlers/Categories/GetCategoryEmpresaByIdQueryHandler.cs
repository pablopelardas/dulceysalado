using MediatR;
using DistriCatalogoAPI.Application.Queries.Categories;
using DistriCatalogoAPI.Domain.Interfaces;
using DistriCatalogoAPI.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace DistriCatalogoAPI.Application.Handlers.Categories
{
    public class GetCategoryEmpresaByIdQueryHandler : IRequestHandler<GetCategoryEmpresaByIdQuery, GetCategoryEmpresaByIdQueryResult?>
    {
        private readonly ICategoryEmpresaRepository _categoryRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ICompanyRepository _companyRepository;
        private readonly ILogger<GetCategoryEmpresaByIdQueryHandler> _logger;

        public GetCategoryEmpresaByIdQueryHandler(
            ICategoryEmpresaRepository categoryRepository,
            ICurrentUserService currentUserService,
            ICompanyRepository companyRepository,
            ILogger<GetCategoryEmpresaByIdQueryHandler> logger)
        {
            _categoryRepository = categoryRepository;
            _currentUserService = currentUserService;
            _companyRepository = companyRepository;
            _logger = logger;
        }

        public async Task<GetCategoryEmpresaByIdQueryResult?> Handle(GetCategoryEmpresaByIdQuery request, CancellationToken cancellationToken)
        {
            var category = await _categoryRepository.GetByIdAsync(request.Id);
            
            if (category == null)
                return null;

            var currentUser = await _currentUserService.GetCurrentUserAsync();
            var userCompany = await _companyRepository.GetByIdAsync(currentUser.CompanyId);
            
            if (userCompany == null)
            {
                throw new InvalidOperationException("Empresa del usuario no encontrada");
            }

            // Validar permisos según el mismo patrón que otros handlers
            if (category.EmpresaId == currentUser.CompanyId)
            {
                // La categoría pertenece a su propia empresa - siempre permitir
                _logger.LogInformation("Usuario {UserId} accediendo a categoría {CategoryId} de su propia empresa {EmpresaId}", 
                    currentUser.Id, request.Id, category.EmpresaId);
            }
            else
            {
                // La categoría pertenece a otra empresa - solo empresas principales pueden verla
                if (!userCompany.IsPrincipal)
                {
                    throw new UnauthorizedAccessException("Solo empresas principales pueden ver categorías de otras empresas");
                }

                // Verificar que la empresa de la categoría es cliente de la empresa principal
                var categoryOwnerCompany = await _companyRepository.GetByIdAsync(category.EmpresaId);
                if (categoryOwnerCompany == null)
                {
                    throw new InvalidOperationException($"Empresa propietaria de la categoría {category.EmpresaId} no encontrada");
                }

                if (!userCompany.CanManageClientCompanyProducts(categoryOwnerCompany))
                {
                    throw new UnauthorizedAccessException($"La empresa {userCompany.Nombre} no puede ver categorías de la empresa {categoryOwnerCompany.Nombre}");
                }

                _logger.LogInformation("Empresa principal {EmpresaId} accediendo a categoría {CategoryId} de empresa cliente {TargetEmpresaId}", 
                    currentUser.CompanyId, request.Id, category.EmpresaId);
            }

            var productCount = await _categoryRepository.CountProductsAsync(request.Id);

            return new GetCategoryEmpresaByIdQueryResult
            {
                Id = category.Id,
                EmpresaId = category.EmpresaId,
                CodigoRubro = category.CodigoRubro,
                Nombre = category.Nombre,
                Icono = category.Icono,
                Visible = category.Visible,
                Orden = category.Orden,
                Color = category.Color,
                Descripcion = category.Descripcion,
                ProductCount = productCount,
                CreatedAt = category.CreatedAt,
                UpdatedAt = category.UpdatedAt
            };
        }
    }
}