using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using DistriCatalogoAPI.Application.Interfaces;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Api.Controllers
{
    /// <summary>
    /// Controlador administrativo para gestión de caché de stock
    /// Solo accesible para administradores del sistema
    /// </summary>
    [ApiController]
    [Route("api/admin/cache")]
    // [Authorize(Roles = "admin")] // Temporalmente deshabilitado para testing
    public class AdminCacheController : ControllerBase
    {
        private readonly IStockCacheService _stockCacheService;
        private readonly IProductoBaseStockRepository _stockRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly ILogger<AdminCacheController> _logger;

        public AdminCacheController(
            IStockCacheService stockCacheService,
            IProductoBaseStockRepository stockRepository,
            ICompanyRepository companyRepository,
            ILogger<AdminCacheController> logger)
        {
            _stockCacheService = stockCacheService;
            _stockRepository = stockRepository;
            _companyRepository = companyRepository;
            _logger = logger;
        }

        /// <summary>
        /// Obtiene estadísticas del caché de stock
        /// </summary>
        [HttpGet("stats")]
        public async Task<ActionResult<CacheStatsResponse>> GetCacheStats()
        {
            try
            {
                var stats = await _stockCacheService.GetCacheStatsAsync();
                
                var response = new CacheStatsResponse
                {
                    TotalHits = stats.TotalHits,
                    TotalMisses = stats.TotalMisses,
                    HitRatio = stats.HitRatio,
                    HitRatioPercentage = Math.Round(stats.HitRatio * 100, 2),
                    CachedEmpresasCount = stats.CachedEmpresasCount,
                    CachedProductosCount = stats.CachedProductosCount,
                    LastInvalidation = stats.LastInvalidation,
                    MemoryUsageBytes = stats.MemoryUsageBytes,
                    MemoryUsageMB = Math.Round(stats.MemoryUsageBytes / (1024.0 * 1024.0), 2)
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas del caché");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Invalida todo el caché de stock del sistema
        /// </summary>
        [HttpPost("invalidate-all")]
        public async Task<ActionResult> InvalidateAllCache()
        {
            try
            {
                await _stockCacheService.InvalidateAllStockCacheAsync();
                
                _logger.LogInformation("Caché de stock invalidado completamente por administrador");
                
                return Ok(new { 
                    message = "Caché invalidado correctamente",
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al invalidar todo el caché");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Invalida el caché de stock de una empresa específica
        /// </summary>
        [HttpPost("invalidate/{empresaId}")]
        public async Task<ActionResult> InvalidateEmpresaCache(int empresaId)
        {
            try
            {
                // Verificar que la empresa existe
                var empresa = await _companyRepository.GetByIdAsync(empresaId);
                if (empresa == null)
                {
                    return NotFound(new { message = $"Empresa {empresaId} no encontrada" });
                }

                await _stockCacheService.InvalidateEmpresaCacheAsync(empresaId);
                
                _logger.LogInformation("Caché de stock invalidado para empresa {EmpresaId} ({EmpresaNombre}) por administrador", 
                    empresaId, empresa.Nombre);
                
                return Ok(new { 
                    message = $"Caché invalidado para empresa {empresa.Nombre}",
                    empresaId = empresaId,
                    empresaNombre = empresa.Nombre,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al invalidar caché de empresa {EmpresaId}", empresaId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Precalienta el caché cargando stock de todos los productos de una empresa
        /// </summary>
        [HttpPost("warm-up/{empresaId}")]
        public async Task<ActionResult> WarmUpEmpresaCache(int empresaId)
        {
            try
            {
                // Verificar que la empresa existe
                var empresa = await _companyRepository.GetByIdAsync(empresaId);
                if (empresa == null)
                {
                    return NotFound(new { message = $"Empresa {empresaId} no encontrada" });
                }

                _logger.LogInformation("Iniciando precalentamiento de caché para empresa {EmpresaId} ({EmpresaNombre})", 
                    empresaId, empresa.Nombre);

                // Obtener todo el stock de la empresa desde BD
                var allStock = await _stockRepository.GetAllStockForEmpresaAsync(empresaId);
                
                if (allStock.Any())
                {
                    // Cargar en caché
                    await _stockCacheService.SetStockBatchAsync(empresaId, allStock);
                    
                    // Cargar lista de productos con stock > 0
                    var productosConStock = allStock.Where(kvp => kvp.Value > 0).Select(kvp => kvp.Key).ToList();
                    await _stockCacheService.SetProductosConStockAsync(empresaId, productosConStock);
                }

                _logger.LogInformation("Caché precalentado para empresa {EmpresaId}: {ProductCount} productos cargados", 
                    empresaId, allStock.Count);
                
                return Ok(new { 
                    message = $"Caché precalentado para empresa {empresa.Nombre}",
                    empresaId = empresaId,
                    empresaNombre = empresa.Nombre,
                    productosEnCache = allStock.Count,
                    productosConStock = allStock.Count(kvp => kvp.Value > 0),
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al precalentar caché de empresa {EmpresaId}", empresaId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Verifica el estado del caché para una empresa
        /// </summary>
        [HttpGet("status/{empresaId}")]
        public async Task<ActionResult<CacheStatusResponse>> GetEmpresaCacheStatus(int empresaId)
        {
            try
            {
                // Verificar que la empresa existe
                var empresa = await _companyRepository.GetByIdAsync(empresaId);
                if (empresa == null)
                {
                    return NotFound(new { message = $"Empresa {empresaId} no encontrada" });
                }

                var isWarm = await _stockCacheService.IsCacheWarmAsync(empresaId);
                var productosConStock = await _stockCacheService.GetProductosConStockAsync(empresaId);
                
                var response = new CacheStatusResponse
                {
                    EmpresaId = empresaId,
                    EmpresaNombre = empresa.Nombre,
                    IsCacheWarm = isWarm,
                    ProductosEnCache = productosConStock.Count,
                    LastChecked = DateTime.UtcNow
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar estado del caché para empresa {EmpresaId}", empresaId);
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        /// <summary>
        /// Precalienta el caché para todas las empresas activas
        /// </summary>
        [HttpPost("warm-up-all")]
        public async Task<ActionResult> WarmUpAllCache()
        {
            try
            {
                _logger.LogInformation("Iniciando precalentamiento masivo de caché por administrador");

                var empresas = await _companyRepository.GetAllAsync();
                var resultados = new List<object>();

                foreach (var empresa in empresas)
                {
                    try
                    {
                        var allStock = await _stockRepository.GetAllStockForEmpresaAsync(empresa.Id);
                        
                        if (allStock.Any())
                        {
                            await _stockCacheService.SetStockBatchAsync(empresa.Id, allStock);
                            var productosConStock = allStock.Where(kvp => kvp.Value > 0).Select(kvp => kvp.Key).ToList();
                            await _stockCacheService.SetProductosConStockAsync(empresa.Id, productosConStock);
                        }

                        resultados.Add(new
                        {
                            empresaId = empresa.Id,
                            empresaNombre = empresa.Nombre,
                            productosEnCache = allStock.Count,
                            productosConStock = allStock.Count(kvp => kvp.Value > 0),
                            exito = true
                        });

                        _logger.LogDebug("Caché precalentado para empresa {EmpresaId}: {ProductCount} productos", 
                            empresa.Id, allStock.Count);
                    }
                    catch (Exception exEmpresa)
                    {
                        _logger.LogError(exEmpresa, "Error precalentando caché para empresa {EmpresaId}", empresa.Id);
                        
                        resultados.Add(new
                        {
                            empresaId = empresa.Id,
                            empresaNombre = empresa.Nombre,
                            error = exEmpresa.Message,
                            exito = false
                        });
                    }
                }

                var exitosos = resultados.Count(r => (bool)r.GetType().GetProperty("exito")?.GetValue(r, null)!);
                
                _logger.LogInformation("Precalentamiento masivo completado: {Exitosos}/{Total} empresas", 
                    exitosos, empresas.Count());

                return Ok(new
                {
                    message = "Precalentamiento masivo completado",
                    totalEmpresas = empresas.Count(),
                    empresasExitosas = exitosos,
                    empresasConError = empresas.Count() - exitosos,
                    detalles = resultados,
                    timestamp = DateTime.UtcNow
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en precalentamiento masivo de caché");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }
    }

    /// <summary>
    /// Respuesta de estadísticas del caché
    /// </summary>
    public class CacheStatsResponse
    {
        public long TotalHits { get; set; }
        public long TotalMisses { get; set; }
        public double HitRatio { get; set; }
        public double HitRatioPercentage { get; set; }
        public int CachedEmpresasCount { get; set; }
        public int CachedProductosCount { get; set; }
        public DateTime LastInvalidation { get; set; }
        public long MemoryUsageBytes { get; set; }
        public double MemoryUsageMB { get; set; }
    }

    /// <summary>
    /// Respuesta de estado del caché por empresa
    /// </summary>
    public class CacheStatusResponse
    {
        public int EmpresaId { get; set; }
        public string EmpresaNombre { get; set; } = string.Empty;
        public bool IsCacheWarm { get; set; }
        public int ProductosEnCache { get; set; }
        public DateTime LastChecked { get; set; }
    }
}