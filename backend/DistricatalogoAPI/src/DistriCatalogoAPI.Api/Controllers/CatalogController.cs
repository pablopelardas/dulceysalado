using Microsoft.AspNetCore.Mvc;
using MediatR;
using DistriCatalogoAPI.Application.Queries.Catalog;
using DistriCatalogoAPI.Api.Services;
using DistriCatalogoAPI.Domain.Entities;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Api.Controllers
{
    [ApiController]
    [Route("api/catalog")]
    public class CatalogController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly CompanyResolverService _companyResolver;
        private readonly ILogger<CatalogController> _logger;

        public CatalogController(
            IMediator mediator, 
            CompanyResolverService companyResolver,
            ILogger<CatalogController> logger)
        {
            _mediator = mediator;
            _companyResolver = companyResolver;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene la empresa actual del contexto, query param o la resuelve desde el host
        /// </summary>
        private async Task<Company> GetCurrentCompanyAsync(int? empresaIdOverride = null)
        {
            // Si se proporciona empresaId como override, usarlo para testing
            if (empresaIdOverride.HasValue)
            {
                _logger.LogDebug("Using empresaId override {EmpresaId} for testing", empresaIdOverride.Value);
                var companyRepository = HttpContext.RequestServices.GetRequiredService<ICompanyRepository>();
                var overrideCompany = await companyRepository.GetByIdAsync(empresaIdOverride.Value);
                
                if (overrideCompany != null)
                {
                    // Cachear la empresa override en el contexto
                    HttpContext.Items["CurrentCompany"] = overrideCompany;
                    HttpContext.Items["CurrentCompanyId"] = overrideCompany.Id;
                    _logger.LogDebug("Override company {CompanyId} ({CompanyName}) resolved successfully", 
                        overrideCompany.Id, overrideCompany.Nombre);
                    return overrideCompany;
                }
                else
                {
                    _logger.LogWarning("Override empresaId {EmpresaId} not found, falling back to domain resolution", empresaIdOverride.Value);
                }
            }

            // Intentar obtener la empresa desde el middleware
            if (HttpContext.Items["CurrentCompany"] is Company cachedCompany)
            {
                return cachedCompany;
            }

            // Si no está en el contexto, resolver desde el host
            var host = HttpContext.Request.Host.Value;
            var company = await _companyResolver.ResolveCompanyFromHostAsync(host);
            
            // Cachear en el contexto para próximas llamadas en la misma request
            HttpContext.Items["CurrentCompany"] = company;
            HttpContext.Items["CurrentCompanyId"] = company.Id;
            
            return company;
        }

        /// <summary>
        /// API pública para obtener catálogo de empresa con filtros
        /// Resuelve automáticamente la empresa desde el subdominio
        /// </summary>
        /// <param name="empresaId">Override para testing - si se proporciona, sobrescribe la resolución por subdominio</param>
        /// <param name="listaPrecioCodigo">Código de la lista de precios (ej: "MAY", "MIN"). Si se omite, usa la lista predeterminada</param>
        /// <param name="ordenarPor">Ordenamiento: precio_asc, precio_desc, nombre_asc, nombre_desc (por defecto: destacados primero, luego por nombre)</param>
        [HttpGet]
        public async Task<ActionResult<GetPublicCatalogQueryResult>> GetPublicCatalog(
            [FromQuery] int? empresaId = null,
            [FromQuery] string? listaPrecioCodigo = null,
            [FromQuery] string? categoria = null,
            [FromQuery] string? busqueda = null,
            [FromQuery] bool? destacados = null,
            [FromQuery] int? codigoRubro = null,
            [FromQuery] string? ordenarPor = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var company = await GetCurrentCompanyAsync(empresaId);
                var resolvedEmpresaId = company.Id;
                
                var query = new GetPublicCatalogQuery
                {
                    EmpresaId = resolvedEmpresaId,
                    ListaPrecioCodigo = listaPrecioCodigo,
                    Categoria = categoria,
                    Busqueda = busqueda,
                    Destacados = destacados,
                    CodigoRubro = codigoRubro,
                    OrdenarPor = ordenarPor,
                    Page = page,
                    PageSize = Math.Min(pageSize, 100) // Limitar a máximo 100 elementos por página
                };

                var result = await _mediator.Send(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                var host = HttpContext.Request.Host.Value;
                _logger.LogError(ex, "Error al obtener catálogo público para host {Host}", host);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtener categorías disponibles para una empresa (API pública)
        /// Resuelve automáticamente la empresa desde el subdominio
        /// </summary>
        /// <param name="empresaId">Override para testing - si se proporciona, sobrescribe la resolución por subdominio</param>
        [HttpGet("categorias")]
        public async Task<ActionResult<GetPublicCategoriesQueryResult>> GetPublicCategories(
            [FromQuery] int? empresaId = null)
        {
            try
            {
                var company = await GetCurrentCompanyAsync(empresaId);
                var resolvedEmpresaId = company.Id;
                var query = new GetPublicCategoriesQuery { EmpresaId = resolvedEmpresaId };
                var result = await _mediator.Send(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                var host = HttpContext.Request.Host.Value;
                _logger.LogError(ex, "Error al obtener categorías públicas para host {Host}", host);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtener detalles de un producto específico (API pública)
        /// Resuelve automáticamente la empresa desde el subdominio
        /// </summary>
        /// <param name="empresaId">Override para testing - si se proporciona, sobrescribe la resolución por subdominio</param>
        [HttpGet("producto/{productoCodigo}")]
        public async Task<ActionResult<GetPublicProductDetailsQueryResult>> GetProductDetails(
            string productoCodigo,
            [FromQuery] int? empresaId = null,
            [FromQuery] string? listaPrecioCodigo= null)
        {
            try
            {
                var company = await GetCurrentCompanyAsync(empresaId);
                var resolvedEmpresaId = company.Id;
                var query = new GetPublicProductDetailsQuery 
                { 
                    EmpresaId = resolvedEmpresaId, 
                    ListaPrecioCodigo = listaPrecioCodigo,
                    ProductoCodigo = productoCodigo 
                };
                
                var result = await _mediator.Send(query);
                
                if (result == null)
                    return NotFound();
                    
                return Ok(result);
            }
            catch (Exception ex)
            {
                var host = HttpContext.Request.Host.Value;
                _logger.LogError(ex, "Error al obtener detalles del producto {ProductoCodigo} para host {Host}", 
                    productoCodigo, host);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Búsqueda avanzada de productos (API pública)
        /// Resuelve automáticamente la empresa desde el subdominio
        /// </summary>
        /// <param name="empresaId">Override para testing - si se proporciona, sobrescribe la resolución por subdominio</param>
        [HttpPost("buscar")]
        public async Task<ActionResult<SearchPublicCatalogQueryResult>> SearchCatalog(
            [FromBody] SearchPublicCatalogQuery query,
            [FromQuery] int? empresaId = null)
        {
            try
            {
                var company = await GetCurrentCompanyAsync(empresaId);
                var resolvedEmpresaId = company.Id;
                query.EmpresaId = resolvedEmpresaId;
                query.PageSize = Math.Min(query.PageSize, 100); // Limitar a máximo 100 elementos por página
                
                var result = await _mediator.Send(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                var host = HttpContext.Request.Host.Value;
                _logger.LogError(ex, "Error en búsqueda avanzada para host {Host}", host);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtener productos destacados de una empresa (API pública)
        /// Resuelve automáticamente la empresa desde el subdominio
        /// </summary>
        /// <param name="empresaId">Override para testing - si se proporciona, sobrescribe la resolución por subdominio</param>
        [HttpGet("destacados")]
        public async Task<ActionResult<GetFeaturedProductsQueryResult>> GetFeaturedProducts(
            [FromQuery] int? empresaId = null,
            [FromQuery] string? listaPrecioCodigo = null,
            [FromQuery] int limit = 10)
        {
            try
            {
                var company = await GetCurrentCompanyAsync(empresaId);
                var resolvedEmpresaId = company.Id;
                var query = new GetFeaturedProductsQuery 
                { 
                    EmpresaId = resolvedEmpresaId,
                    ListaPrecioCodigo = listaPrecioCodigo,
                    Limit = Math.Min(limit, 50) // Limitar a máximo 50 productos destacados
                };
                
                var result = await _mediator.Send(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                var host = HttpContext.Request.Host.Value;
                _logger.LogError(ex, "Error al obtener productos destacados para host {Host}", host);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtener productos sin configuración web completa para gestión administrativa
        /// Resuelve automáticamente la empresa desde el subdominio
        /// </summary>
        /// <param name="empresaId">Override para testing - si se proporciona, sobrescribe la resolución por subdominio</param>
        [HttpGet("sin-configuracion")]
        public async Task<ActionResult<GetUnconfiguredProductsQueryResult>> GetUnconfiguredProducts(
            [FromQuery] int? empresaId = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var company = await GetCurrentCompanyAsync(empresaId);
                var resolvedEmpresaId = company.Id;
                var query = new GetUnconfiguredProductsQuery 
                { 
                    EmpresaId = resolvedEmpresaId,
                    Page = page,
                    PageSize = Math.Min(pageSize, 100)
                };
                
                var result = await _mediator.Send(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                var host = HttpContext.Request.Host.Value;
                _logger.LogError(ex, "Error al obtener productos sin configuración para host {Host}", host);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtener información y configuración de la empresa (API pública)
        /// Resuelve automáticamente la empresa desde el subdominio
        /// </summary>
        /// <param name="empresaId">Override para testing - si se proporciona, sobrescribe la resolución por subdominio</param>
        [HttpGet("empresa")]
        public async Task<ActionResult<GetPublicCompanyInfoQueryResult>> GetCompanyInfo(
            [FromQuery] int? empresaId = null)
        {
            try
            {
                var company = await GetCurrentCompanyAsync(empresaId);
                var resolvedEmpresaId = company.Id;
                var query = new GetPublicCompanyInfoQuery { EmpresaId = resolvedEmpresaId };
                var result = await _mediator.Send(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                var host = HttpContext.Request.Host.Value;
                _logger.LogError(ex, "Error al obtener información de empresa para host {Host}", host);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtener productos novedades de una empresa (API pública sin paginación)
        /// Resuelve automáticamente la empresa desde el subdominio
        /// </summary>
        /// <param name="empresaId">Override para testing - si se proporciona, sobrescribe la resolución por subdominio</param>
        /// <param name="listaPrecioCodigo">Código de la lista de precios (ej: "MAY", "MIN"). Si se omite, usa la lista predeterminada</param>
        /// <param name="ordenarPor">Ordenamiento: precio_asc, precio_desc, nombre_asc, nombre_desc (por defecto: destacados primero, luego por nombre)</param>
        [HttpGet("novedades")]
        public async Task<ActionResult<GetProductosNovedadesQueryResult>> GetProductosNovedades(
            [FromQuery] int? empresaId = null,
            [FromQuery] string? listaPrecioCodigo = null,
            [FromQuery] string? ordenarPor = null)
        {
            try
            {
                var company = await GetCurrentCompanyAsync(empresaId);
                var resolvedEmpresaId = company.Id;
                
                var query = new GetProductosNovedadesQuery
                {
                    EmpresaId = resolvedEmpresaId,
                    ListaPrecioCodigo = listaPrecioCodigo,
                    OrdenarPor = ordenarPor
                };

                var result = await _mediator.Send(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                var host = HttpContext.Request.Host.Value;
                _logger.LogError(ex, "Error al obtener productos novedades para host {Host}", host);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Obtener productos ofertas de una empresa (API pública sin paginación)
        /// Resuelve automáticamente la empresa desde el subdominio
        /// </summary>
        /// <param name="empresaId">Override para testing - si se proporciona, sobrescribe la resolución por subdominio</param>
        /// <param name="listaPrecioCodigo">Código de la lista de precios (ej: "MAY", "MIN"). Si se omite, usa la lista predeterminada</param>
        /// <param name="ordenarPor">Ordenamiento: precio_asc, precio_desc, nombre_asc, nombre_desc (por defecto: destacados primero, luego por nombre)</param>
        [HttpGet("ofertas")]
        public async Task<ActionResult<GetProductosOfertasQueryResult>> GetProductosOfertas(
            [FromQuery] int? empresaId = null,
            [FromQuery] string? listaPrecioCodigo = null,
            [FromQuery] string? ordenarPor = null)
        {
            try
            {
                var company = await GetCurrentCompanyAsync(empresaId);
                var resolvedEmpresaId = company.Id;
                
                var query = new GetProductosOfertasQuery
                {
                    EmpresaId = resolvedEmpresaId,
                    ListaPrecioCodigo = listaPrecioCodigo,
                    OrdenarPor = ordenarPor
                };

                var result = await _mediator.Send(query);
                return Ok(result);
            }
            catch (Exception ex)
            {
                var host = HttpContext.Request.Host.Value;
                _logger.LogError(ex, "Error al obtener productos ofertas para host {Host}", host);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }
    }
}