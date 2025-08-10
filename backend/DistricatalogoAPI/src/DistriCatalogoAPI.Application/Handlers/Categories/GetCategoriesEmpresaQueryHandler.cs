using MediatR;
using DistriCatalogoAPI.Application.Queries.Categories;
using DistriCatalogoAPI.Domain.Interfaces;
using DistriCatalogoAPI.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace DistriCatalogoAPI.Application.Handlers.Categories
{
    public class GetCategoriesEmpresaQueryHandler : IRequestHandler<GetCategoriesEmpresaQuery, GetCategoriesEmpresaQueryResult>
    {
        private readonly ICategoryEmpresaRepository _categoryRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ICompanyRepository _companyRepository;
        private readonly ILogger<GetCategoriesEmpresaQueryHandler> _logger;

        public GetCategoriesEmpresaQueryHandler(
            ICategoryEmpresaRepository categoryRepository,
            ICurrentUserService currentUserService,
            ICompanyRepository companyRepository,
            ILogger<GetCategoriesEmpresaQueryHandler> logger)
        {
            _categoryRepository = categoryRepository;
            _currentUserService = currentUserService;
            _companyRepository = companyRepository;
            _logger = logger;
        }

        public async Task<GetCategoriesEmpresaQueryResult> Handle(GetCategoriesEmpresaQuery request, CancellationToken cancellationToken)
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync();
            var userCompany = await _companyRepository.GetByIdAsync(currentUser.CompanyId);
            
            if (userCompany == null)
            {
                throw new InvalidOperationException("Empresa del usuario no encontrada");
            }

            // Determinar qué empresas puede ver el usuario según la misma lógica que ProductosEmpresa
            int? empresaIdFiltro = null;

            // Si no se especifica empresaId en el request
            if (request.EmpresaId <= 0)
            {
                // Empresas principales pueden ver categorías de todas las empresas (sin filtro)
                if (userCompany.IsPrincipal)
                {
                    empresaIdFiltro = null; // Ver todas las empresas
                    _logger.LogInformation("Empresa principal {EmpresaId} obteniendo categorías de todas las empresas", currentUser.CompanyId);
                }
                else
                {
                    // Empresas cliente solo pueden ver sus propias categorías
                    empresaIdFiltro = currentUser.CompanyId;
                    _logger.LogInformation("Empresa cliente {EmpresaId} obteniendo sus propias categorías", currentUser.CompanyId);
                }
            }
            else
            {
                // Si se especifica una empresa concreta
                var targetEmpresaId = request.EmpresaId;
                
                // Si consulta su propia empresa, permitir
                if (targetEmpresaId == currentUser.CompanyId)
                {
                    empresaIdFiltro = targetEmpresaId;
                }
                else
                {
                    // Solo empresas principales pueden ver categorías de otras empresas
                    if (!userCompany.IsPrincipal)
                    {
                        throw new UnauthorizedAccessException("Solo empresas principales pueden ver categorías de otras empresas");
                    }

                    // Verificar que la empresa objetivo sea válida
                    var targetCompany = await _companyRepository.GetByIdAsync(targetEmpresaId);
                    if (targetCompany == null)
                    {
                        throw new InvalidOperationException($"Empresa objetivo {targetEmpresaId} no encontrada");
                    }

                    if (!userCompany.CanManageClientCompanyProducts(targetCompany))
                    {
                        throw new UnauthorizedAccessException($"La empresa {userCompany.Nombre} no puede ver categorías de la empresa {targetCompany.Nombre}");
                    }
                    
                    empresaIdFiltro = targetEmpresaId;
                    _logger.LogInformation("Empresa principal {EmpresaId} obteniendo categorías de empresa {TargetEmpresaId}", currentUser.CompanyId, targetEmpresaId);
                }
            }

            // Obtener categorías según el filtro determinado
            IReadOnlyList<Domain.Entities.CategoryEmpresa> categories;
            
            if (empresaIdFiltro.HasValue)
            {
                // Obtener categorías de una empresa específica
                if (request.VisibleOnly == true)
                {
                    categories = await _categoryRepository.GetVisibleByEmpresaIdAsync(empresaIdFiltro.Value);
                }
                else
                {
                    categories = await _categoryRepository.GetByEmpresaIdAsync(empresaIdFiltro.Value);
                }
            }
            else
            {
                // Obtener categorías de todas las empresas (solo empresas principales pueden llegar aquí)
                categories = await _categoryRepository.GetAllAsync();
                _logger.LogInformation("Obteniendo categorías de todas las empresas para empresa principal");
            }

            var categoryDtos = new List<CategoryEmpresaDto>();
            foreach (var category in categories)
            {
                var productCount = await _categoryRepository.CountProductsAsync(category.Id);
                
                categoryDtos.Add(new CategoryEmpresaDto
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
                });
            }

            _logger.LogInformation("Se encontraron {Count} categorías empresa", categoryDtos.Count);

            return new GetCategoriesEmpresaQueryResult
            {
                Categories = categoryDtos
            };
        }
    }
}