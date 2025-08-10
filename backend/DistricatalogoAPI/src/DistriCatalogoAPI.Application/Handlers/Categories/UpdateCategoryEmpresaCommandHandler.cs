using MediatR;
using DistriCatalogoAPI.Application.Commands.Categories;
using DistriCatalogoAPI.Domain.Interfaces;
using DistriCatalogoAPI.Application.Interfaces;
using Microsoft.Extensions.Logging;

namespace DistriCatalogoAPI.Application.Handlers.Categories
{
    public class UpdateCategoryEmpresaCommandHandler : IRequestHandler<UpdateCategoryEmpresaCommand, UpdateCategoryEmpresaCommandResult>
    {
        private readonly ICategoryEmpresaRepository _categoryRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ICompanyRepository _companyRepository;
        private readonly ILogger<UpdateCategoryEmpresaCommandHandler> _logger;

        public UpdateCategoryEmpresaCommandHandler(
            ICategoryEmpresaRepository categoryRepository,
            ICurrentUserService currentUserService,
            ICompanyRepository companyRepository,
            ILogger<UpdateCategoryEmpresaCommandHandler> logger)
        {
            _categoryRepository = categoryRepository;
            _currentUserService = currentUserService;
            _companyRepository = companyRepository;
            _logger = logger;
        }

        public async Task<UpdateCategoryEmpresaCommandResult> Handle(UpdateCategoryEmpresaCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Actualizando categoría empresa - ID: {Id}, CodigoRubro: {CodigoRubro}, Nombre: {Nombre}", 
                request.Id, request.CodigoRubro, request.Nombre);

            // Buscar categoría existente
            var existingCategory = await _categoryRepository.GetByIdAsync(request.Id);
            if (existingCategory == null)
                throw new InvalidOperationException($"Categoría con ID {request.Id} no encontrada");

            var currentUser = await _currentUserService.GetCurrentUserAsync();
            var userCompany = await _companyRepository.GetByIdAsync(currentUser.CompanyId);
            
            if (userCompany == null)
            {
                throw new InvalidOperationException("Empresa del usuario no encontrada");
            }

            // Validar permisos según el mismo patrón que otros handlers
            if (existingCategory.EmpresaId == currentUser.CompanyId)
            {
                // La categoría pertenece a su propia empresa - siempre permitir
                _logger.LogInformation("Usuario {UserId} actualizando categoría {CategoryId} de su propia empresa {EmpresaId}", 
                    currentUser.Id, request.Id, existingCategory.EmpresaId);
            }
            else
            {
                // La categoría pertenece a otra empresa - solo empresas principales pueden editarla
                if (!userCompany.IsPrincipal)
                {
                    throw new UnauthorizedAccessException("Solo empresas principales pueden actualizar categorías de otras empresas");
                }

                // Verificar que la empresa de la categoría es cliente de la empresa principal
                var categoryOwnerCompany = await _companyRepository.GetByIdAsync(existingCategory.EmpresaId);
                if (categoryOwnerCompany == null)
                {
                    throw new InvalidOperationException($"Empresa propietaria de la categoría {existingCategory.EmpresaId} no encontrada");
                }

                if (!userCompany.CanManageClientCompanyProducts(categoryOwnerCompany))
                {
                    throw new UnauthorizedAccessException($"La empresa {userCompany.Nombre} no puede actualizar categorías de la empresa {categoryOwnerCompany.Nombre}");
                }

                _logger.LogInformation("Empresa principal {EmpresaId} actualizando categoría {CategoryId} de empresa cliente {TargetEmpresaId}", 
                    currentUser.CompanyId, request.Id, existingCategory.EmpresaId);
            }

            // Validar código de rubro si se está enviando uno
            if (request.CodigoRubro.HasValue)
            {
                var codigoRubroFinal = request.CodigoRubro.Value;
                
                _logger.LogInformation("Validando código de rubro {CodigoRubro} para categoría {CategoryId} en empresa {EmpresaId}", 
                    codigoRubroFinal, request.Id, existingCategory.EmpresaId);

                // Verificar que no existe otra categoría empresa con el mismo código de rubro en la misma empresa
                var conflictCategory = await _categoryRepository.GetByEmpresaAndCodigoRubroAsync(existingCategory.EmpresaId, codigoRubroFinal);
                
                if (conflictCategory != null && conflictCategory.Id != existingCategory.Id)
                {
                    _logger.LogWarning("Conflicto encontrado: categoría {ConflictId} ya usa código {CodigoRubro} en empresa {EmpresaId}", 
                        conflictCategory.Id, codigoRubroFinal, existingCategory.EmpresaId);
                    
                    throw new InvalidOperationException($"Ya existe una categoría con el código de rubro {codigoRubroFinal} en esta empresa (ID: {conflictCategory.Id})");
                }

                if (codigoRubroFinal != existingCategory.CodigoRubro)
                {
                    _logger.LogInformation("Cambiando código de rubro de categoría {CategoryId} de {OldCodigo} a {NewCodigo}", 
                        request.Id, existingCategory.CodigoRubro, codigoRubroFinal);
                }
                else
                {
                    _logger.LogInformation("Manteniendo código de rubro {CodigoRubro} para categoría {CategoryId}", 
                        codigoRubroFinal, request.Id);
                }
            }

            // Actualizar campos usando el método Update de la entidad
            existingCategory.Update(
                nombre: request.Nombre,
                icono: request.Icono,
                visible: request.Visible,
                orden: request.Orden,
                color: request.Color,
                descripcion: request.Descripcion,
                codigoRubro: request.CodigoRubro
            );

            var updatedCategory = await _categoryRepository.UpdateAsync(existingCategory);

            return new UpdateCategoryEmpresaCommandResult
            {
                Id = updatedCategory.Id,
                EmpresaId = updatedCategory.EmpresaId,
                CodigoRubro = updatedCategory.CodigoRubro,
                Nombre = updatedCategory.Nombre,
                Icono = updatedCategory.Icono,
                Visible = updatedCategory.Visible,
                Orden = updatedCategory.Orden,
                Color = updatedCategory.Color,
                Descripcion = updatedCategory.Descripcion,
                UpdatedAt = updatedCategory.UpdatedAt
            };
        }
    }
}