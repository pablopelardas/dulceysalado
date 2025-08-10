using MediatR;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Application.Commands.ProductosEmpresa;
using DistriCatalogoAPI.Domain.Interfaces;
using DistriCatalogoAPI.Application.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.ProductosEmpresa
{
    public class DeleteProductoEmpresaCommandHandler : IRequestHandler<DeleteProductoEmpresaCommand, bool>
    {
        private readonly IProductoEmpresaRepository _productRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<DeleteProductoEmpresaCommandHandler> _logger;

        public DeleteProductoEmpresaCommandHandler(
            IProductoEmpresaRepository productRepository,
            ICompanyRepository companyRepository,
            ICurrentUserService currentUserService,
            ILogger<DeleteProductoEmpresaCommandHandler> logger)
        {
            _productRepository = productRepository;
            _companyRepository = companyRepository;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<bool> Handle(DeleteProductoEmpresaCommand request, CancellationToken cancellationToken)
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync();

            var product = await _productRepository.GetByIdAsync(request.Id);
            if (product == null)
            {
                _logger.LogWarning("Intento de eliminar producto empresa inexistente - Id: {Id}", request.Id);
                return false;
            }

            // VALIDACIÓN: El usuario puede eliminar productos de su empresa o de empresas cliente si es empresa principal
            var userCompany = await _companyRepository.GetByIdAsync(currentUser.CompanyId);
            if (userCompany == null)
            {
                throw new InvalidOperationException("Empresa del usuario no encontrada");
            }

            // Si el producto pertenece a la empresa del usuario
            if (product.BelongsToCompany(currentUser.CompanyId))
            {
                if (!currentUser.CanManageCompanyProducts)
                {
                    throw new UnauthorizedAccessException("No tiene permisos para eliminar productos de su empresa");
                }
            }
            // Si el producto pertenece a otra empresa
            else
            {
                // Solo empresas principales pueden eliminar productos de otras empresas
                if (!userCompany.IsPrincipal)
                {
                    throw new UnauthorizedAccessException("Solo empresas principales pueden eliminar productos de otras empresas");
                }

                // Verificar que la empresa del producto sea cliente de la empresa principal
                var productCompany = await _companyRepository.GetByIdAsync(product.EmpresaId);
                if (productCompany == null)
                {
                    throw new InvalidOperationException($"Empresa del producto {product.EmpresaId} no encontrada");
                }

                if (!userCompany.CanManageClientCompanyProducts(productCompany))
                {
                    throw new UnauthorizedAccessException($"La empresa {userCompany.Nombre} no puede gestionar productos de la empresa {productCompany.Nombre}");
                }

                if (!currentUser.CanManageCompanyProducts)
                {
                    throw new UnauthorizedAccessException("No tiene permisos para gestionar productos de empresas");
                }
            }

            _logger.LogInformation("Eliminando producto empresa - Id: {Id}, EmpresaId: {EmpresaId}, Codigo: {Codigo}, Usuario: {UserId}", 
                request.Id, product.EmpresaId, product.Codigo, currentUser.Id);

            await _productRepository.DeleteAsync(product);

            _logger.LogInformation("Producto empresa eliminado exitosamente - Id: {ProductId}", request.Id);

            return true;
        }
    }
}