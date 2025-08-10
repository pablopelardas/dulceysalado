using MediatR;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Application.Commands.ProductosBase;
using DistriCatalogoAPI.Domain.Entities;
using DistriCatalogoAPI.Domain.Interfaces;
using DistriCatalogoAPI.Application.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.ProductosBase
{
    public class CreateProductoBaseCommandHandler : IRequestHandler<CreateProductoBaseCommand, CreateProductoBaseCommandResult>
    {
        private readonly IProductBaseRepository _productRepository;
        private readonly ICategoryBaseRepository _categoryRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<CreateProductoBaseCommandHandler> _logger;

        public CreateProductoBaseCommandHandler(
            IProductBaseRepository productRepository,
            ICategoryBaseRepository categoryRepository,
            ICompanyRepository companyRepository,
            ICurrentUserService currentUserService,
            ILogger<CreateProductoBaseCommandHandler> logger)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
            _companyRepository = companyRepository;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<CreateProductoBaseCommandResult> Handle(CreateProductoBaseCommand request, CancellationToken cancellationToken)
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync();
            
            // VALIDACIÓN CRÍTICA: Solo empresas principales pueden crear productos base
            var company = await _companyRepository.GetByIdAsync(currentUser.CompanyId);
            if (company == null)
            {
                throw new InvalidOperationException("Empresa no encontrada");
            }

            if (!company.IsPrincipal)
            {
                throw new UnauthorizedAccessException("Solo empresas principales pueden crear productos base. Las empresas cliente deben usar ProductosEmpresa.");
            }

            if (!currentUser.CanManageBaseProducts)
            {
                throw new UnauthorizedAccessException("No tiene permisos para gestionar productos base");
            }

            // Validar que no exista ya un producto con ese código globalmente
            var existingProduct = await _productRepository.GetByCodigoAsync(request.Codigo);
            if (existingProduct != null)
            {
                throw new InvalidOperationException($"Ya existe un producto base con el código {request.Codigo}");
            }

            // VALIDACIÓN: Si se especifica CodigoRubro, debe existir SOLO en categorías base de la empresa principal
            if (request.CodigoRubro.HasValue)
            {
                var categoryExists = await _categoryRepository.CategoryExistsInPrincipalCompanyAsync(request.CodigoRubro.Value, company.Id);
                if (!categoryExists)
                {
                    throw new InvalidOperationException($"El código de rubro {request.CodigoRubro} no existe en las categorías base de la empresa principal");
                }
            }

            _logger.LogInformation("Creando producto base - Codigo: {Codigo}, Descripcion: {Descripcion}, Usuario: {UserId}", 
                request.Codigo, request.Descripcion, currentUser.Id);

            // Crear el nuevo producto usando el método factory de la entidad
            var product = ProductBase.CreateFromSync(
                request.Codigo,
                request.Descripcion,
                request.CodigoRubro,
                request.Precio ?? 0,
                request.Existencia ?? 0,
                null, // grupo1
                null, // grupo2
                null, // grupo3
                DateTime.UtcNow, // fechaAlta
                DateTime.UtcNow, // fechaModi
                "S", // imputable
                "S", // disponible
                null, // codigoUbicacion
                currentUser.CompanyId); // Usar la empresa del usuario autenticado

            // Actualizar campos web si fueron proporcionados
            product.UpdateWebFields(
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

            _logger.LogInformation("Producto base creado exitosamente - Id: {ProductId}, Codigo: {Codigo}", 
                createdProduct.Id, createdProduct.Codigo);

            return new CreateProductoBaseCommandResult
            {
                Id = createdProduct.Id,
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
                AdministradoPorEmpresaId = createdProduct.AdministradoPorEmpresaId,
                CreatedAt = createdProduct.CreatedAt,
                UpdatedAt = createdProduct.UpdatedAt
            };
        }
    }
}