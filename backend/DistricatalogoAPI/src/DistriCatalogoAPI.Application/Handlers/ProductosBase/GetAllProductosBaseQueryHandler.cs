using System.Linq;
using MediatR;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Application.Queries.ProductosBase;
using DistriCatalogoAPI.Domain.Interfaces;
using DistriCatalogoAPI.Application.Interfaces;

namespace DistriCatalogoAPI.Application.Handlers.ProductosBase
{
    public class GetAllProductosBaseQueryHandler : IRequestHandler<GetAllProductosBaseQuery, GetAllProductosBaseQueryResult>
    {
        private readonly IProductBaseRepository _productRepository;
        private readonly IListaPrecioRepository _listaPrecioRepository;
        private readonly IProductBasePrecioRepository _precioRepository;
        private readonly IProductoBaseStockRepository _stockRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetAllProductosBaseQueryHandler> _logger;

        public GetAllProductosBaseQueryHandler(
            IProductBaseRepository productRepository,
            IListaPrecioRepository listaPrecioRepository,
            IProductBasePrecioRepository precioRepository,
            IProductoBaseStockRepository stockRepository,
            ICompanyRepository companyRepository,
            ICurrentUserService currentUserService,
            ILogger<GetAllProductosBaseQueryHandler> logger)
        {
            _productRepository = productRepository;
            _listaPrecioRepository = listaPrecioRepository;
            _precioRepository = precioRepository;
            _stockRepository = stockRepository;
            _companyRepository = companyRepository;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<GetAllProductosBaseQueryResult> Handle(GetAllProductosBaseQuery request, CancellationToken cancellationToken)
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync();

            _logger.LogInformation("Obteniendo productos base - Usuario: {UserId}, Page: {Page}, PageSize: {PageSize}, ListaPrecio: {ListaPrecio}", 
                currentUser.Id, request.Page, request.PageSize, request.ListaPrecioId);

            // Obtener todas las listas de precios disponibles
            var listasDisponibles = await _listaPrecioRepository.GetAllActiveAsync();
            _logger.LogInformation("Listas de precios disponibles: {Count}", listasDisponibles.Count);

            // Determinar la lista seleccionada
            var listaSeleccionada = listasDisponibles.FirstOrDefault(l => l.Id == request.ListaPrecioId)
                ?? listasDisponibles.FirstOrDefault(l => l.EsPredeterminada)
                ?? listasDisponibles.FirstOrDefault();

            if (listaSeleccionada != null)
            {
                _logger.LogInformation("Lista seleccionada: {ListaId} - {ListaCodigo} - {ListaNombre}", 
                    listaSeleccionada.Id, listaSeleccionada.Codigo, listaSeleccionada.Nombre);
            }
            else
            {
                _logger.LogWarning("No se encontró ninguna lista de precios activa");
            }

            // Determinar empresa para filtrar stock
            var empresaId = request.EmpresaId ?? await GetEmpresaPrincipalIdAsync();
            _logger.LogInformation("Empresa seleccionada para stock: {EmpresaId}", empresaId);

            // Obtener todos los productos primero, luego filtraremos por stock
            var (allProducts, totalBeforeStockFilter) = await _productRepository.GetPagedAsync(
                request.Visible,
                request.Destacado,
                request.CodigoRubro,
                request.Busqueda,
                1, // Obtener todo para filtrar por stock
                int.MaxValue, // Sin límite inicial
                request.SortBy,
                request.SortOrder,
                listaSeleccionada?.Id,
                true, // Incluir todos los productos inicialmente
                request.SoloSinConfiguracion);

            List<Domain.Entities.ProductBase> products;
            int total;

            // Si solo queremos productos con stock, filtrar después de obtener el stock
            if (!request.IncluirSinExistencia)
            {
                // Obtener stock para todos los productos
                var productIds = allProducts.Select(p => p.Id).ToList();
                var stockPorProducto = await _stockRepository.GetStockBatchAsync(empresaId, productIds);
                
                // Filtrar productos que tienen stock > 0
                products = allProducts.Where(p => stockPorProducto.ContainsKey(p.Id) && stockPorProducto[p.Id] > 0).ToList();
                total = products.Count;
                
                _logger.LogInformation("Productos filtrados por stock - Total antes: {TotalBefore}, Total después: {TotalAfter}", 
                    allProducts.Count, products.Count);
                
                // Aplicar paginación después del filtro de stock
                products = products.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToList();
            }
            else
            {
                // Sin filtro de stock, aplicar paginación normal
                total = totalBeforeStockFilter;
                products = allProducts.Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToList();
            }

            var productDtos = new List<ProductoBaseDto>();

            // Si hay una lista seleccionada, obtener precios para los productos
            Dictionary<int, decimal> preciosPorProducto = new();
            if (listaSeleccionada != null && products.Any())
            {
                var productIds = products.Select(p => p.Id).ToList();
                _logger.LogInformation("Obteniendo precios para {ProductCount} productos en lista {ListaId}", 
                    productIds.Count, listaSeleccionada.Id);
                
                preciosPorProducto = await _precioRepository.GetPreciosPorProductosYListaAsync(productIds, listaSeleccionada.Id);
                
                _logger.LogInformation("Se encontraron {PrecioCount} precios para los productos", 
                    preciosPorProducto.Count);
            }

            // Obtener stock para los productos finales (si no se obtuvo ya arriba)
            Dictionary<int, decimal> stockPorProductoFinal = new();
            if (products.Any())
            {
                var productIds = products.Select(p => p.Id).ToList();
                stockPorProductoFinal = await _stockRepository.GetStockBatchAsync(empresaId, productIds);
                
                _logger.LogInformation("Se encontraron {StockCount} stocks para los productos finales", 
                    stockPorProductoFinal.Count);
            }

            // Mapear productos con precios, stock y configuraciones faltantes
            productDtos = products.Select(p => new ProductoBaseDto
            {
                Id = p.Id,
                Codigo = p.Codigo,
                Descripcion = p.Descripcion,
                CodigoRubro = p.CodigoRubro,
                Existencia = stockPorProductoFinal.GetValueOrDefault(p.Id, 0m), // Usar stock del nuevo repositorio
                Visible = p.Visible,
                Destacado = p.Destacado,
                OrdenCategoria = p.OrdenCategoria,
                ImagenUrl = p.ImagenUrl,
                ImagenAlt = p.ImagenAlt,
                DescripcionCorta = p.DescripcionCorta,
                DescripcionLarga = p.DescripcionLarga,
                Tags = p.Tags,
                CodigoBarras = p.CodigoBarras,
                Marca = p.Marca,
                UnidadMedida = p.UnidadMedida,
                AdministradoPorEmpresaId = p.AdministradoPorEmpresaId,
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                PrecioSeleccionado = preciosPorProducto.GetValueOrDefault(p.Id),
                ConfiguracionesFaltantes = GetMissingConfiguration(p)
            }).ToList();

            // Los filtros ya se aplicaron en el repositorio

            return new GetAllProductosBaseQueryResult
            {
                Productos = productDtos,
                Total = total,
                Page = request.Page,
                PageSize = request.PageSize,
                TotalPages = (int)Math.Ceiling((double)total / request.PageSize),
                ListaSeleccionada = listaSeleccionada != null ? new ListaPrecioInfo
                {
                    Id = listaSeleccionada.Id,
                    Codigo = listaSeleccionada.Codigo,
                    Nombre = listaSeleccionada.Nombre,
                    EsPredeterminada = listaSeleccionada.EsPredeterminada
                } : null,
                ListasDisponibles = listasDisponibles.Select(l => new ListaPrecioInfo
                {
                    Id = l.Id,
                    Codigo = l.Codigo,
                    Nombre = l.Nombre,
                    EsPredeterminada = l.EsPredeterminada
                }).ToList()
            };
        }

        private List<string> GetMissingConfiguration(Domain.Entities.ProductBase product)
        {
            var missing = new List<string>();

            if (string.IsNullOrEmpty(product.ImagenUrl))
                missing.Add("Imagen principal");

            return missing;
        }

        private async Task<int> GetEmpresaPrincipalIdAsync()
        {
            // Obtener la primera empresa activa como empresa principal por defecto
            var empresasActivas = await _companyRepository.GetAllAsync(includeInactive: false);
            var empresaPrincipal = empresasActivas.FirstOrDefault();
            
            if (empresaPrincipal == null)
            {
                _logger.LogWarning("No se encontró ninguna empresa activa para usar como principal");
                return 1; // Fallback a ID 1
            }

            _logger.LogDebug("Empresa principal determinada: {EmpresaId}", empresaPrincipal.Id);
            return empresaPrincipal.Id;
        }
    }
}