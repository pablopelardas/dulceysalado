using MediatR;
using DistriCatalogoAPI.Application.Commands.Categories;
using DistriCatalogoAPI.Domain.Interfaces;
using DistriCatalogoAPI.Application.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.Categories
{
    public class DeleteCategoryEmpresaCommandHandler : IRequestHandler<DeleteCategoryEmpresaCommand, bool>
    {
        private readonly ICategoryEmpresaRepository _categoryRepository;
        private readonly ICurrentUserService _currentUserService;

        public DeleteCategoryEmpresaCommandHandler(
            ICategoryEmpresaRepository categoryRepository,
            ICurrentUserService currentUserService)
        {
            _categoryRepository = categoryRepository;
            _currentUserService = currentUserService;
        }

        public async Task<bool> Handle(DeleteCategoryEmpresaCommand request, CancellationToken cancellationToken)
        {
            var currentCompanyId = _currentUserService.GetCurrentCompanyId();
            
            // Buscar categoría existente
            var existingCategory = await _categoryRepository.GetByIdAsync(request.Id);
            if (existingCategory == null)
                return false;

            // Verificar que pertenece a la empresa del usuario actual
            if (existingCategory.EmpresaId != currentCompanyId)
                throw new UnauthorizedAccessException("No tiene permisos para eliminar esta categoría");

            // Verificar que no tenga productos asociados
            var productCount = await _categoryRepository.CountProductsAsync(request.Id);
            if (productCount > 0)
                throw new InvalidOperationException($"No se puede eliminar la categoría porque tiene {productCount} productos asociados");

            await _categoryRepository.DeleteAsync(request.Id);
            return true;
        }
    }
}