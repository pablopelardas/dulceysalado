using MediatR;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Application.Commands.ProductosEmpresa;
using DistriCatalogoAPI.Domain.Interfaces;
using DistriCatalogoAPI.Application.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.ProductosEmpresa
{
    public class UpdateProductoEmpresaCommandHandler : IRequestHandler<UpdateProductoEmpresaCommand, UpdateProductoEmpresaCommandResult>
    {
        private readonly IProductoEmpresaRepository _productRepository;
        private readonly IProductBaseRepository _productBaseRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<UpdateProductoEmpresaCommandHandler> _logger;

        public UpdateProductoEmpresaCommandHandler(
            IProductoEmpresaRepository productRepository,
            IProductBaseRepository productBaseRepository,
            ICompanyRepository companyRepository,
            ICurrentUserService currentUserService,
            ILogger<UpdateProductoEmpresaCommandHandler> logger)
        {
            _productRepository = productRepository;
            _productBaseRepository = productBaseRepository;
            _companyRepository = companyRepository;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<UpdateProductoEmpresaCommandResult> Handle(UpdateProductoEmpresaCommand request, CancellationToken cancellationToken)
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync();

            var product = await _productRepository.GetByIdAsync(request.Id);
            if (product == null)
            {
                throw new InvalidOperationException($"No se encontró el producto con ID {request.Id}");
            }

            // VALIDACIÓN: El usuario puede actualizar productos de su empresa o de empresas cliente si es empresa principal
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
                    throw new UnauthorizedAccessException("No tiene permisos para actualizar productos de su empresa");
                }
            }
            // Si el producto pertenece a otra empresa
            else
            {
                // Solo empresas principales pueden actualizar productos de otras empresas
                if (!userCompany.IsPrincipal)
                {
                    throw new UnauthorizedAccessException("Solo empresas principales pueden actualizar productos de otras empresas");
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

            _logger.LogInformation("Actualizando producto empresa - Id: {Id}, EmpresaId: {EmpresaId}, Usuario: {UserId}", 
                request.Id, product.EmpresaId, currentUser.Id);

            // Actualizar usando el método de la entidad de dominio
            product.Update(
                request.Descripcion,
                request.CodigoRubro,
                request.Precio,
                request.Existencia,
                request.Visible,
                request.Destacado,
                request.OrdenCategoria,
                request.ImagenUrl,
                request.ImagenAlt,
                request.DescripcionCorta,
                request.DescripcionLarga,
                request.Tags,
                request.CodigoBarras,
                request.Marca,
                request.UnidadMedida);

            await _productRepository.UpdateAsync(product);

            _logger.LogInformation("Producto empresa actualizado exitosamente - Id: {ProductId}", product.Id);

            return new UpdateProductoEmpresaCommandResult
            {
                Id = product.Id,
                EmpresaId = product.EmpresaId,
                EmpresaNombre = null, // Se cargará desde el repositorio si es necesario
                Codigo = product.Codigo,
                Descripcion = product.Descripcion,
                CodigoRubro = product.CodigoRubro,
                Precio = product.Precio,
                Existencia = product.Existencia,
                Visible = product.Visible,
                Destacado = product.Destacado,
                OrdenCategoria = product.OrdenCategoria,
                ImagenUrl = product.ImagenUrl,
                ImagenAlt = product.ImagenAlt,
                DescripcionCorta = product.DescripcionCorta,
                DescripcionLarga = product.DescripcionLarga,
                Tags = product.Tags,
                CodigoBarras = product.CodigoBarras,
                Marca = product.Marca,
                UnidadMedida = product.UnidadMedida,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };
        }
    }
}