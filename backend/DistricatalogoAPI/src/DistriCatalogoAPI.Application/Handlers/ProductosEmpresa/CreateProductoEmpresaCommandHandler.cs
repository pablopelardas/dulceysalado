using MediatR;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Application.Commands.ProductosEmpresa;
using DistriCatalogoAPI.Domain.Interfaces;
using DistriCatalogoAPI.Application.Interfaces;
using DistriCatalogoAPI.Domain.Entities;

namespace DistriCatalogoAPI.Application.Handlers.ProductosEmpresa
{
    public class CreateProductoEmpresaCommandHandler : IRequestHandler<CreateProductoEmpresaCommand, CreateProductoEmpresaCommandResult>
    {
        private readonly IProductoEmpresaRepository _productRepository;
        private readonly IProductBaseRepository _productBaseRepository;
        private readonly ICategoryEmpresaRepository _categoryRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<CreateProductoEmpresaCommandHandler> _logger;

        public CreateProductoEmpresaCommandHandler(
            IProductoEmpresaRepository productRepository,
            IProductBaseRepository productBaseRepository,
            ICategoryEmpresaRepository categoryRepository,
            ICompanyRepository companyRepository,
            ICurrentUserService currentUserService,
            ILogger<CreateProductoEmpresaCommandHandler> logger)
        {
            _productRepository = productRepository;
            _productBaseRepository = productBaseRepository;
            _categoryRepository = categoryRepository;
            _companyRepository = companyRepository;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<CreateProductoEmpresaCommandResult> Handle(CreateProductoEmpresaCommand request, CancellationToken cancellationToken)
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync();
            
            // VALIDACIÓN: El usuario puede crear productos para su empresa o para empresas cliente si es empresa principal
            var userCompany = await _companyRepository.GetByIdAsync(currentUser.CompanyId);
            if (userCompany == null)
            {
                throw new InvalidOperationException("Empresa del usuario no encontrada");
            }

            // Si el usuario intenta crear para su propia empresa
            if (request.EmpresaId == currentUser.CompanyId)
            {
                if (!currentUser.CanManageCompanyProducts)
                {
                    throw new UnauthorizedAccessException("No tiene permisos para crear productos en su empresa");
                }
            }
            // Si el usuario intenta crear para otra empresa
            else
            {
                // Solo empresas principales pueden crear productos para otras empresas
                if (!userCompany.IsPrincipal)
                {
                    throw new UnauthorizedAccessException("Solo empresas principales pueden crear productos para otras empresas");
                }

                // Verificar que la empresa objetivo sea cliente de la empresa principal
                var targetCompany = await _companyRepository.GetByIdAsync(request.EmpresaId);
                if (targetCompany == null)
                {
                    throw new InvalidOperationException($"Empresa objetivo {request.EmpresaId} no encontrada");
                }

                if (!userCompany.CanManageClientCompanyProducts(targetCompany))
                {
                    throw new UnauthorizedAccessException($"La empresa {userCompany.Nombre} no puede gestionar productos de la empresa {targetCompany.Nombre}");
                }

                if (!currentUser.CanManageCompanyProducts)
                {
                    throw new UnauthorizedAccessException("No tiene permisos para gestionar productos de empresas");
                }
            }

            // Validar que no exista ya un producto con ese código en esta empresa
            var existingProduct = await _productRepository.ExistsByCodigoAsync(request.Codigo, request.EmpresaId);
            if (existingProduct)
            {
                throw new InvalidOperationException($"Ya existe un producto con el código {request.Codigo} en esta empresa");
            }

            // VALIDACIÓN CRÍTICA: El código no puede existir en productos base de la misma empresa principal
            var empresaPrincipalId = userCompany.IsPrincipal ? userCompany.Id : userCompany.EmpresaPrincipalId!.Value;
            var existsInProductBase = await _productBaseRepository.ExistsByCodigoInPrincipalCompanyAsync(request.Codigo, empresaPrincipalId);
            if (existsInProductBase)
            {
                throw new InvalidOperationException($"El código {request.Codigo} ya existe en productos base de la empresa principal");
            }

            // VALIDACIÓN CRÍTICA: El código no puede existir en otros productos empresa de la misma empresa principal
            var existsInOtherProductEmpresa = await _productRepository.ExistsByCodigoInPrincipalCompanyAsync(request.Codigo, empresaPrincipalId);
            if (existsInOtherProductEmpresa)
            {
                throw new InvalidOperationException($"El código {request.Codigo} ya existe en productos de empresa de la empresa principal");
            }

            // VALIDACIÓN: Si se especifica CodigoRubro, debe existir en categorías base de empresa principal O categorías de la empresa específica
            if (request.CodigoRubro.HasValue)
            {
                var categoryExists = await _categoryRepository.CategoryExistsForCompanyAsync(request.CodigoRubro.Value, request.EmpresaId, empresaPrincipalId);
                if (!categoryExists)
                {
                    throw new InvalidOperationException($"El código de rubro {request.CodigoRubro} no existe en las categorías disponibles para esta empresa");
                }
            }

            _logger.LogInformation("Creando producto empresa - EmpresaId: {EmpresaId}, Codigo: {Codigo}, Usuario: {UserId}", 
                request.EmpresaId, request.Codigo, currentUser.Id);

            var product = ProductoEmpresa.Create(
                request.EmpresaId,
                request.Codigo,
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

            var createdProduct = await _productRepository.CreateAsync(product);

            _logger.LogInformation("Producto empresa creado exitosamente - Id: {ProductId}, Codigo: {Codigo}", 
                createdProduct.Id, createdProduct.Codigo);

            return new CreateProductoEmpresaCommandResult
            {
                Id = createdProduct.Id,
                EmpresaId = createdProduct.EmpresaId,
                EmpresaNombre = null, // Se cargará desde el repositorio si es necesario
                Codigo = createdProduct.Codigo,
                Descripcion = createdProduct.Descripcion,
                CodigoRubro = createdProduct.CodigoRubro,
                Precio = createdProduct.Precio,
                Existencia = createdProduct.Existencia,
                Visible = createdProduct.Visible,
                Destacado = createdProduct.Destacado,
                OrdenCategoria = createdProduct.OrdenCategoria,
                ImagenUrl = createdProduct.ImagenUrl,
                ImagenAlt = createdProduct.ImagenAlt,
                DescripcionCorta = createdProduct.DescripcionCorta,
                DescripcionLarga = createdProduct.DescripcionLarga,
                Tags = createdProduct.Tags,
                CodigoBarras = createdProduct.CodigoBarras,
                Marca = createdProduct.Marca,
                UnidadMedida = createdProduct.UnidadMedida,
                CreatedAt = createdProduct.CreatedAt,
                UpdatedAt = createdProduct.UpdatedAt
            };
        }
    }
}