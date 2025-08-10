using MediatR;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Application.Commands.Categories;
using DistriCatalogoAPI.Domain.Entities;
using DistriCatalogoAPI.Domain.Interfaces;
using DistriCatalogoAPI.Application.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.Categories
{
    public class CreateCategoryBaseCommandHandler : IRequestHandler<CreateCategoryBaseCommand, CreateCategoryBaseCommandResult>
    {
        private readonly ICategoryBaseRepository _categoryRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<CreateCategoryBaseCommandHandler> _logger;

        public CreateCategoryBaseCommandHandler(
            ICategoryBaseRepository categoryRepository,
            ICurrentUserService currentUserService,
            ILogger<CreateCategoryBaseCommandHandler> logger)
        {
            _categoryRepository = categoryRepository;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<CreateCategoryBaseCommandResult> Handle(CreateCategoryBaseCommand request, CancellationToken cancellationToken)
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync();
            
            // Validar permisos - Solo empresa principal puede crear categorías base
            if (!currentUser.CanManageBaseCategories || !currentUser.IsFromPrincipalCompany)
            {
                throw new UnauthorizedAccessException("No tiene permisos para crear categorías base");
            }

            // Validar que no exista ya una categoría con ese código de rubro
            var existingCategory = await _categoryRepository.GetByCodigoRubroAsync(request.CodigoRubro);
            if (existingCategory != null)
            {
                throw new InvalidOperationException($"Ya existe una categoría base con el código de rubro {request.CodigoRubro}");
            }

            _logger.LogInformation("Creando categoría base - CodigoRubro: {CodigoRubro}, Nombre: {Nombre}, Usuario: {UserId}", 
                request.CodigoRubro, request.Nombre, currentUser.Id);

            // Crear la nueva categoría
            var category = CategoryBase.Create(
                request.CodigoRubro,
                request.Nombre,
                request.Icono,
                request.Visible,
                request.Orden,
                request.Color ?? "#6B7280",
                request.Descripcion,
                currentUser.CompanyId);

            var createdCategory = await _categoryRepository.CreateAsync(category);

            _logger.LogInformation("Categoría base creada exitosamente - Id: {CategoryId}, CodigoRubro: {CodigoRubro}", 
                createdCategory.Id, createdCategory.CodigoRubro);

            return new CreateCategoryBaseCommandResult
            {
                Id = createdCategory.Id,
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