using System.Linq;
using MediatR;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Application.Queries.ProductosEmpresa;
using DistriCatalogoAPI.Domain.Interfaces;
using DistriCatalogoAPI.Application.Interfaces;
using ListaPrecioInfo = DistriCatalogoAPI.Application.Queries.ProductosBase.ListaPrecioInfo;

namespace DistriCatalogoAPI.Application.Handlers.ProductosEmpresa
{
    public class GetProductosEmpresaByEmpresaQueryHandler : IRequestHandler<GetProductosEmpresaByEmpresaQuery, GetProductosEmpresaByEmpresaQueryResult>
    {
        private readonly IProductoEmpresaRepository _productRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IListaPrecioRepository _listaPrecioRepository;
        private readonly IProductoEmpresaPrecioRepository _precioRepository;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<GetProductosEmpresaByEmpresaQueryHandler> _logger;

        public GetProductosEmpresaByEmpresaQueryHandler(
            IProductoEmpresaRepository productRepository,
            ICompanyRepository companyRepository,
            IListaPrecioRepository listaPrecioRepository,
            IProductoEmpresaPrecioRepository precioRepository,
            ICurrentUserService currentUserService,
            ILogger<GetProductosEmpresaByEmpresaQueryHandler> logger)
        {
            _productRepository = productRepository;
            _companyRepository = companyRepository;
            _listaPrecioRepository = listaPrecioRepository;
            _precioRepository = precioRepository;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        public async Task<GetProductosEmpresaByEmpresaQueryResult> Handle(GetProductosEmpresaByEmpresaQuery request, CancellationToken cancellationToken)
        {
            var currentUser = await _currentUserService.GetCurrentUserAsync();

            // VALIDACIÓN: Determinar qué empresas puede ver el usuario
            var userCompany = await _companyRepository.GetByIdAsync(currentUser.CompanyId);
            if (userCompany == null)
            {
                throw new InvalidOperationException("Empresa del usuario no encontrada");
            }

            int? empresaIdFiltro = null;

            // Si no se especifica empresaId (request.EmpresaId es null)
            if (!request.EmpresaId.HasValue)
            {
                // Empresas principales pueden ver productos de todas las empresas (sin filtro)
                if (userCompany.IsPrincipal)
                {
                    // No filtrar por empresa - ver todos los productos
                    empresaIdFiltro = null;
                }
                else
                {
                    // Empresas cliente solo pueden ver sus propios productos
                    empresaIdFiltro = currentUser.CompanyId;
                }
            }
            else
            {
                // Si se especifica una empresa concreta
                var targetEmpresaId = request.EmpresaId.Value;
                
                // Si consulta su propia empresa, permitir
                if (targetEmpresaId == currentUser.CompanyId)
                {
                    empresaIdFiltro = targetEmpresaId;
                }
                else
                {
                    // Solo empresas principales pueden ver productos de otras empresas
                    if (!userCompany.IsPrincipal)
                    {
                        throw new UnauthorizedAccessException("Solo empresas principales pueden ver productos de otras empresas");
                    }

                    // Verificar que la empresa objetivo sea válida
                    var targetCompany = await _companyRepository.GetByIdAsync(targetEmpresaId);
                    if (targetCompany == null)
                    {
                        throw new InvalidOperationException($"Empresa objetivo {targetEmpresaId} no encontrada");
                    }

                    if (!userCompany.CanManageClientCompanyProducts(targetCompany))
                    {
                        throw new UnauthorizedAccessException($"La empresa {userCompany.Nombre} no puede ver productos de la empresa {targetCompany.Nombre}");
                    }
                    
                    empresaIdFiltro = targetEmpresaId;
                }
            }

            _logger.LogInformation("Obteniendo productos empresa - EmpresaIdFiltro: {EmpresaIdFiltro}, Usuario: {UserId}, Page: {Page}, ListaPrecio: {ListaPrecio}", 
                empresaIdFiltro, currentUser.Id, request.Page, request.ListaPrecioId);

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

            var (products, total) = await _productRepository.GetPagedByEmpresaAsync(
                empresaIdFiltro,
                request.Visible,
                request.Destacado,
                request.CodigoRubro,
                request.Busqueda,
                request.Page,
                request.PageSize,
                request.SortBy,
                request.SortOrder,
                listaSeleccionada?.Id,
                request.IncluirSinExistencia);

            var productDtos = new List<ProductoEmpresaDto>();

            // Si hay una lista seleccionada, obtener precios para los productos
            Dictionary<int, decimal?> preciosPorProducto = new();
            if (listaSeleccionada != null && products.Any())
            {
                var productIds = products.Select(p => p.Id).ToList();
                _logger.LogInformation("Obteniendo precios para {ProductCount} productos empresa en lista {ListaId}", 
                    productIds.Count, listaSeleccionada.Id);
                
                preciosPorProducto = await _precioRepository.GetPreciosPorProductosYListaAsync(productIds, listaSeleccionada.Id);
                
                _logger.LogInformation("Se encontraron {PrecioCount} precios para los productos empresa", 
                    preciosPorProducto.Count);
            }

            // Mapear productos con precios y configuraciones faltantes
            productDtos = products.Select(p => new ProductoEmpresaDto
            {
                Id = p.Id,
                EmpresaId = p.EmpresaId,
                EmpresaNombre = null, // Se cargará desde el repositorio si es necesario
                Codigo = p.Codigo,
                Descripcion = p.Descripcion,
                CodigoRubro = p.CodigoRubro,
                PrecioSeleccionado = preciosPorProducto.TryGetValue(p.Id, out var precio) ? precio : null,
                Existencia = p.Existencia,
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
                CreatedAt = p.CreatedAt,
                UpdatedAt = p.UpdatedAt,
                ConfiguracionesFaltantes = GetMissingConfiguration(p)
            }).ToList();

            // El filtro de existencia ya se aplicó en el repositorio

            // Aplicar filtro de solo sin configuración si está especificado
            if (request.SoloSinConfiguracion == true)
            {
                productDtos = productDtos.Where(p => p.ConfiguracionesFaltantes.Any()).ToList();
                total = productDtos.Count;
                _logger.LogInformation("Filtro aplicado: solo productos sin configuración. Total filtrado: {Total}", total);
            }

            return new GetProductosEmpresaByEmpresaQueryResult
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

        private List<string> GetMissingConfiguration(Domain.Entities.ProductoEmpresa product)
        {
            var missing = new List<string>();

            if (string.IsNullOrEmpty(product.ImagenUrl))
                missing.Add("Imagen principal");

            return missing;
        }
    }
}