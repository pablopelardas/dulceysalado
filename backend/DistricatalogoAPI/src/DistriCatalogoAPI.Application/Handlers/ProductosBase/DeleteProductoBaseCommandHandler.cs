using MediatR;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Application.Commands.ProductosBase;
using DistriCatalogoAPI.Domain.Interfaces;
using DistriCatalogoAPI.Application.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.ProductosBase
{
    public class DeleteProductoBaseCommandHandler : IRequestHandler<DeleteProductoBaseCommand, bool>
    {
        private readonly IProductBaseRepository _productRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<DeleteProductoBaseCommandHandler> _logger;

        public DeleteProductoBaseCommandHandler(
            IProductBaseRepository productRepository,
            ICompanyRepository companyRepository,
            ICurrentUserService currentUserService,
            ILogger<DeleteProductoBaseCommandHandler> logger)
        {
            _productRepository = productRepository;
            _companyRepository = companyRepository;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<bool> Handle(DeleteProductoBaseCommand request, CancellationToken cancellationToken)
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync();
            
            // VALIDACIÓN CRÍTICA: Solo empresas principales pueden eliminar productos base
            var company = await _companyRepository.GetByIdAsync(currentUser.CompanyId);
            if (company == null)
            {
                throw new InvalidOperationException("Empresa no encontrada");
            }

            if (!company.IsPrincipal)
            {
                throw new UnauthorizedAccessException("Solo empresas principales pueden eliminar productos base");
            }

            if (!currentUser.CanManageBaseProducts)
            {
                throw new UnauthorizedAccessException("No tiene permisos para gestionar productos base");
            }

            var product = await _productRepository.GetByIdAsync(request.Id);
            if (product == null)
            {
                _logger.LogWarning("Intento de eliminar producto inexistente - Id: {Id}", request.Id);
                return false;
            }

            _logger.LogInformation("Eliminando producto base - Id: {Id}, Codigo: {Codigo}, Usuario: {UserId}", 
                request.Id, product.Codigo, currentUser.Id);

            await _productRepository.DeleteAsync(product);

            _logger.LogInformation("Producto base eliminado exitosamente - Id: {ProductId}", request.Id);

            return true;
        }
    }
}