using MediatR;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Application.Commands.Categories;
using DistriCatalogoAPI.Domain.Entities;
using DistriCatalogoAPI.Domain.Interfaces;
using DistriCatalogoAPI.Application.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.Categories
{
    public class UpdateCategoryBaseCommandHandler : IRequestHandler<UpdateCategoryBaseCommand, UpdateCategoryBaseCommandResult>
    {
        private readonly ICategoryBaseRepository _categoryRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<UpdateCategoryBaseCommandHandler> _logger;

        public UpdateCategoryBaseCommandHandler(
            ICategoryBaseRepository categoryRepository,
            ICurrentUserService currentUserService,
            ILogger<UpdateCategoryBaseCommandHandler> logger)
        {
            _categoryRepository = categoryRepository;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<UpdateCategoryBaseCommandResult> Handle(UpdateCategoryBaseCommand request, CancellationToken cancellationToken)
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync();
            
            // Validar permisos - Solo empresa principal puede actualizar categorías base
            if (!currentUser.CanManageBaseCategories || !currentUser.IsFromPrincipalCompany)
            {
                throw new UnauthorizedAccessException("No tiene permisos para actualizar categorías base");
            }

            _logger.LogInformation("Actualizando categoría base - Id: {CategoryId}, Usuario: {UserId}", 
                request.Id, currentUser.Id);

            // Obtener la categoría existente
            var existingCategory = await _categoryRepository.GetByIdAsync(request.Id);
            if (existingCategory == null)
            {
                throw new InvalidOperationException($"Categoría base con ID {request.Id} no encontrada");
            }

            // Crear categoría con datos actualizados usando el mismo CodigoRubro
            var updatedCategory = CategoryBase.Create(
                existingCategory.CodigoRubro, // Mantener el mismo código rubro
                request.Nombre ?? existingCategory.Nombre,
                request.Icono ?? existingCategory.Icono,
                request.Visible ?? existingCategory.Visible,
                request.Orden ?? existingCategory.Orden,
                request.Color ?? existingCategory.Color,
                request.Descripcion ?? existingCategory.Descripcion,
                existingCategory.CreatedByEmpresaId);

            // Establecer el ID correcto usando reflection (manteniendo el patrón del proyecto)
            var idProperty = typeof(CategoryBase).GetProperty("Id");
            if (idProperty != null && idProperty.CanWrite)
            {
                idProperty.SetValue(updatedCategory, request.Id);
            }
            else
            {
                var idField = typeof(CategoryBase).GetField("<Id>k__BackingField", 
                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                idField?.SetValue(updatedCategory, request.Id);
            }

            // Guardar cambios en base de datos
            var savedCategory = await _categoryRepository.UpdateAsync(updatedCategory);

            _logger.LogInformation("Categoría base actualizada exitosamente - Id: {CategoryId}", request.Id);

            return new UpdateCategoryBaseCommandResult
            {
                Id = savedCategory.Id,
                CodigoRubro = savedCategory.CodigoRubro,
                Nombre = savedCategory.Nombre,
                Icono = savedCategory.Icono,
                Visible = savedCategory.Visible,
                Orden = savedCategory.Orden,
                Color = savedCategory.Color,
                Descripcion = savedCategory.Descripcion,
                UpdatedAt = savedCategory.UpdatedAt
            };
        }
    }
}