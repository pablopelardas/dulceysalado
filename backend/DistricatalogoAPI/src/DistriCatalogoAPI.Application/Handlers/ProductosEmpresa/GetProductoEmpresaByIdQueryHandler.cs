using MediatR;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Application.Queries.ProductosEmpresa;
using DistriCatalogoAPI.Domain.Interfaces;
using DistriCatalogoAPI.Application.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.ProductosEmpresa
{
    public class GetProductoEmpresaByIdQueryHandler : IRequestHandler<GetProductoEmpresaByIdQuery, GetProductoEmpresaByIdQueryResult?>
    {
        private readonly IProductoEmpresaRepository _productRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IProductoEmpresaPrecioRepository _precioRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetProductoEmpresaByIdQueryHandler> _logger;

        public GetProductoEmpresaByIdQueryHandler(
            IProductoEmpresaRepository productRepository,
            ICompanyRepository companyRepository,
            IProductoEmpresaPrecioRepository precioRepository,
            ICurrentUserService currentUserService,
            ILogger<GetProductoEmpresaByIdQueryHandler> logger)
        {
            _productRepository = productRepository;
            _companyRepository = companyRepository;
            _precioRepository = precioRepository;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<GetProductoEmpresaByIdQueryResult?> Handle(GetProductoEmpresaByIdQuery request, CancellationToken cancellationToken)
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync();

            _logger.LogInformation("Obteniendo producto empresa por ID: {Id}, Usuario: {UserId}", 
                request.Id, currentUser.Id);

            var product = await _productRepository.GetByIdAsync(request.Id);
            
            if (product == null)
            {
                _logger.LogWarning("Producto empresa no encontrado: {Id}", request.Id);
                return null;
            }

            // VALIDACIÓN: El usuario puede ver productos de su empresa o de empresas cliente si es empresa principal
            var userCompany = await _companyRepository.GetByIdAsync(currentUser.CompanyId);
            if (userCompany == null)
            {
                throw new InvalidOperationException("Empresa del usuario no encontrada");
            }

            // Si el producto pertenece a la empresa del usuario
            if (product.BelongsToCompany(currentUser.CompanyId))
            {
                // OK - puede ver productos de su propia empresa
            }
            // Si el producto pertenece a otra empresa
            else
            {
                // Solo empresas principales pueden ver productos de otras empresas
                if (!userCompany.IsPrincipal)
                {
                    throw new UnauthorizedAccessException("Solo empresas principales pueden ver productos de otras empresas");
                }

                // Verificar que la empresa del producto sea cliente de la empresa principal
                var productCompany = await _companyRepository.GetByIdAsync(product.EmpresaId);
                if (productCompany == null)
                {
                    throw new InvalidOperationException($"Empresa del producto {product.EmpresaId} no encontrada");
                }

                if (!userCompany.CanManageClientCompanyProducts(productCompany))
                {
                    throw new UnauthorizedAccessException($"La empresa {userCompany.Nombre} no puede ver productos de la empresa {productCompany.Nombre}");
                }
            }

            // Obtener todos los precios del producto para todas las listas
            var precios = await _precioRepository.GetPreciosPorProductoAsync(product.Id);
            
            _logger.LogInformation("Producto empresa {ProductoId} tiene {PrecioCount} precios configurados", 
                product.Id, precios.Count);

            return new GetProductoEmpresaByIdQueryResult
            {
                Id = product.Id,
                EmpresaId = product.EmpresaId,
                EmpresaNombre = null, // Se cargará desde el repositorio si es necesario
                Codigo = product.Codigo,
                Descripcion = product.Descripcion,
                CodigoRubro = product.CodigoRubro,
                Precios = precios.Select(p => new DistriCatalogoAPI.Application.Queries.ProductosBase.ProductoPrecioInfo
                {
                    ListaPrecioId = p.ListaPrecioId,
                    ListaPrecioCodigo = p.ListaPrecioCodigo,
                    ListaPrecioNombre = p.ListaPrecioNombre,
                    Precio = p.Precio
                }).ToList(),
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