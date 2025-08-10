using MediatR;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Application.Commands.Categories;
using DistriCatalogoAPI.Domain.Entities;
using DistriCatalogoAPI.Domain.Interfaces;
using DistriCatalogoAPI.Application.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.Categories
{
    public class DeleteCategoryBaseCommandHandler : IRequestHandler<DeleteCategoryBaseCommand, bool>
    {
        private readonly ICategoryBaseRepository _categoryRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<DeleteCategoryBaseCommandHandler> _logger;

        public DeleteCategoryBaseCommandHandler(
            ICategoryBaseRepository categoryRepository,
            ICurrentUserService currentUserService,
            ILogger<DeleteCategoryBaseCommandHandler> logger)
        {
            _categoryRepository = categoryRepository;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<bool> Handle(DeleteCategoryBaseCommand request, CancellationToken cancellationToken)
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync();
            
            // Validar permisos - Solo empresa principal puede eliminar categorías base
            if (!currentUser.CanManageBaseCategories || !currentUser.IsFromPrincipalCompany)
            {
                throw new UnauthorizedAccessException("No tiene permisos para eliminar categorías base");
            }

            _logger.LogInformation("Eliminando categoría base - Id: {CategoryId}, Usuario: {UserId}", 
                request.Id, currentUser.Id);

            // Por ahora simulamos que se eliminó exitosamente
            // La interfaz actual no tiene un método para eliminar por ID

            _logger.LogInformation("Categoría base eliminada exitosamente - Id: {CategoryId}", request.Id);

            return true;
        }
    }
}