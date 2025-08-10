using MediatR;
using DistriCatalogoAPI.Application.Commands.Categories;
using DistriCatalogoAPI.Domain.Interfaces;
using DistriCatalogoAPI.Domain.Entities;
using DistriCatalogoAPI.Application.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.Categories
{
    public class CreateCategoryEmpresaCommandHandler : IRequestHandler<CreateCategoryEmpresaCommand, CreateCategoryEmpresaCommandResult>
    {
        private readonly ICategoryEmpresaRepository _categoryRepository;
        private readonly ICategoryBaseRepository _categoryBaseRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly ICurrentUserService _currentUserService;

        public CreateCategoryEmpresaCommandHandler(
            ICategoryEmpresaRepository categoryRepository,
            ICategoryBaseRepository categoryBaseRepository,
            ICompanyRepository companyRepository,
            ICurrentUserService currentUserService)
        {
            _categoryRepository = categoryRepository;
            _categoryBaseRepository = categoryBaseRepository;
            _companyRepository = companyRepository;
            _currentUserService = currentUserService;
        }

        public async Task<CreateCategoryEmpresaCommandResult> Handle(CreateCategoryEmpresaCommand request, CancellationToken cancellationToken)
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync();
            var currentCompanyId = currentUser.CompanyId;
            
            // Obtener información de la empresa del usuario
            var userCompany = await _companyRepository.GetByIdAsync(currentCompanyId);
            if (userCompany == null)
            {
                throw new InvalidOperationException("Empresa del usuario no encontrada");
            }

            // Determinar la empresa objetivo para la categoría
            var targetEmpresaId = request.EmpresaId > 0 ? request.EmpresaId : currentCompanyId;

            // Si la empresa objetivo es diferente a la del usuario, validar permisos
            if (targetEmpresaId != currentCompanyId)
            {
                // Solo empresas principales pueden crear categorías para otras empresas
                if (!userCompany.IsPrincipal)
                {
                    throw new UnauthorizedAccessException("Solo empresas principales pueden crear categorías para otras empresas");
                }

                // Verificar que la empresa objetivo existe y es cliente de la empresa principal
                var targetCompany = await _companyRepository.GetByIdAsync(targetEmpresaId);
                if (targetCompany == null)
                {
                    throw new InvalidOperationException($"Empresa objetivo {targetEmpresaId} no encontrada");
                }

                if (!userCompany.CanManageClientCompanyProducts(targetCompany))
                {
                    throw new UnauthorizedAccessException($"La empresa {userCompany.Nombre} no puede crear categorías para la empresa {targetCompany.Nombre}");
                }
            }

            // Verificar que no existe ya una categoría con el mismo código de rubro en la empresa objetivo
            var existingCategory = await _categoryRepository.GetByEmpresaAndCodigoRubroAsync(targetEmpresaId, request.CodigoRubro);
            if (existingCategory != null)
                throw new InvalidOperationException($"Ya existe una categoría con el código de rubro {request.CodigoRubro} en la empresa {targetEmpresaId}");

            // VALIDACIÓN CRÍTICA: El CodigoRubro no puede existir en categorías base de la misma empresa principal
            var empresaPrincipalId = userCompany.IsPrincipal ? userCompany.Id : userCompany.EmpresaPrincipalId!.Value;
            var existsInCategoryBase = await _categoryBaseRepository.ExistsByCodigoRubroInPrincipalCompanyAsync(request.CodigoRubro, empresaPrincipalId);
            if (existsInCategoryBase)
            {
                throw new InvalidOperationException($"El código de rubro {request.CodigoRubro} ya existe en categorías base de la empresa principal");
            }

            // VALIDACIÓN CRÍTICA: El CodigoRubro no puede existir en otras categorías empresa de la misma empresa principal
            var existsInOtherCategoryEmpresa = await _categoryRepository.ExistsByCodigoRubroInPrincipalCompanyAsync(request.CodigoRubro, empresaPrincipalId);
            if (existsInOtherCategoryEmpresa)
            {
                throw new InvalidOperationException($"El código de rubro {request.CodigoRubro} ya existe en categorías de empresa de la empresa principal");
            }

            // Crear nueva categoría empresa para la empresa objetivo
            var category = CategoryEmpresa.Create(
                targetEmpresaId,  // Usar empresa objetivo, no la del usuario actual
                request.CodigoRubro,
                request.Nombre,
                request.Icono,
                request.Visible,
                request.Orden,
                request.Color ?? "#6B7280",
                request.Descripcion
            );

            var createdCategory = await _categoryRepository.AddAsync(category);

            return new CreateCategoryEmpresaCommandResult
            {
                Id = createdCategory.Id,
                EmpresaId = createdCategory.EmpresaId,
                CodigoRubro = createdCategory.CodigoRubro,
                Nombre = createdCategory.Nombre,
                Icono = createdCategory.Icono,
                Visible = createdCategory.Visible,
                Orden = createdCategory.Orden,
                Color = createdCategory.Color,
                Descripcion = createdCategory.Descripcion,
                CreatedAt = createdCategory.CreatedAt
            };
        }
    }
}