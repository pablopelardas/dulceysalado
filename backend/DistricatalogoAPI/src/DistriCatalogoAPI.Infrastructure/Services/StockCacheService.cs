using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using DistriCatalogoAPI.Application.Interfaces;
using DistriCatalogoAPI.Infrastructure.Configuration;

namespace DistriCatalogoAPI.Infrastructure.Services
{
    public class StockCacheService : IStockCacheService
    {
        private readonly IMemoryCache _cache;
        private readonly ILogger<StockCacheService> _logger;
        private readonly StockCacheOptions _options;
        
        // Prefijos para claves de caché
        private const string STOCK_KEY_PREFIX = "stock:empresa:{0}:producto:{1}";
        private const string PRODUCTOS_CON_STOCK_KEY = "productos_con_stock:empresa:{0}";
        private const string CACHE_WARM_KEY = "cache_warm:empresa:{0}";
        private const string STATS_KEY = "cache_stats";
        private const string EMPRESA_STOCK_KEYS = "empresa_stock_keys:{0}";
        private const string ALL_EMPRESAS_WITH_CACHE = "all_empresas_with_cache";
        
        // Estadísticas del caché
        private static long _totalHits = 0;
        private static long _totalMisses = 0;
        private static DateTime _lastInvalidation = DateTime.UtcNow;

        public StockCacheService(
            IMemoryCache cache, 
            ILogger<StockCacheService> logger,
            IOptions<StockCacheOptions> options)
        {
            _cache = cache;
            _logger = logger;
            _options = options.Value;
        }

        public async Task<decimal?> GetStockAsync(int empresaId, int productoId)
        {
            var key = string.Format(STOCK_KEY_PREFIX, empresaId, productoId);
            
            if (_cache.TryGetValue(key, out decimal stock))
            {
                Interlocked.Increment(ref _totalHits);
                _logger.LogDebug("Cache HIT para stock - Empresa: {EmpresaId}, Producto: {ProductoId}, Stock: {Stock}", 
                    empresaId, productoId, stock);
                return stock;
            }
            
            Interlocked.Increment(ref _totalMisses);
            _logger.LogDebug("Cache MISS para stock - Empresa: {EmpresaId}, Producto: {ProductoId}", 
                empresaId, productoId);
            return null;
        }

        public async Task SetStockAsync(int empresaId, int productoId, decimal stock)
        {
            var key = string.Format(STOCK_KEY_PREFIX, empresaId, productoId);
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _options.GetTtlTimeSpan(),
                Priority = CacheItemPriority.Normal,
                Size = 1 // Para control de memoria
            };

            _cache.Set(key, stock, options);
            
            _logger.LogDebug("Stock almacenado en caché - Empresa: {EmpresaId}, Producto: {ProductoId}, Stock: {Stock}", 
                empresaId, productoId, stock);
        }

        public async Task<List<int>> GetProductosConStockAsync(int empresaId)
        {
            var key = string.Format(PRODUCTOS_CON_STOCK_KEY, empresaId);
            
            if (_cache.TryGetValue(key, out List<int> productos))
            {
                Interlocked.Increment(ref _totalHits);
                _logger.LogDebug("Cache HIT para productos con stock - Empresa: {EmpresaId}, Productos: {Count}", 
                    empresaId, productos?.Count ?? 0);
                return productos ?? new List<int>();
            }
            
            Interlocked.Increment(ref _totalMisses);
            _logger.LogDebug("Cache MISS para productos con stock - Empresa: {EmpresaId}", empresaId);
            return new List<int>();
        }

        public async Task SetProductosConStockAsync(int empresaId, List<int> productoIds)
        {
            var key = string.Format(PRODUCTOS_CON_STOCK_KEY, empresaId);
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _options.GetTtlTimeSpan(),
                Priority = CacheItemPriority.High, // Mayor prioridad para listas
                Size = Math.Max(1, productoIds.Count / 100) // Tamaño proporcional
            };

            _cache.Set(key, productoIds, options);
            
            // Marcar caché como "caliente" para esta empresa
            var warmKey = string.Format(CACHE_WARM_KEY, empresaId);
            _cache.Set(warmKey, DateTime.UtcNow, options);
            
            _logger.LogInformation("Productos con stock almacenados en caché - Empresa: {EmpresaId}, Productos: {Count}", 
                empresaId, productoIds.Count);
        }

        public async Task<Dictionary<int, decimal>> GetStockBatchAsync(int empresaId, List<int> productoIds)
        {
            var result = new Dictionary<int, decimal>();
            var hits = 0;
            var misses = 0;

            foreach (var productoId in productoIds)
            {
                var key = string.Format(STOCK_KEY_PREFIX, empresaId, productoId);
                if (_cache.TryGetValue(key, out decimal stock))
                {
                    result[productoId] = stock;
                    hits++;
                    
                    // Debug logging para producto específico
                    if (productoId == 5219)
                    {
                        _logger.LogWarning("DEBUG Cache Hit: Producto 5219 - Empresa {EmpresaId}, Stock en caché: {Stock}, Key: {Key}", 
                            empresaId, stock, key);
                    }
                }
                else
                {
                    misses++;
                    
                    // Debug logging para producto específico
                    if (productoId == 5219)
                    {
                        _logger.LogWarning("DEBUG Cache Miss: Producto 5219 - Empresa {EmpresaId}, Key: {Key}", 
                            empresaId, key);
                    }
                }
            }

            // Actualizar estadísticas
            Interlocked.Add(ref _totalHits, hits);
            Interlocked.Add(ref _totalMisses, misses);

            _logger.LogDebug("Batch stock query - Empresa: {EmpresaId}, Solicitados: {Total}, Hits: {Hits}, Misses: {Misses}", 
                empresaId, productoIds.Count, hits, misses);

            return result;
        }

        public async Task SetStockBatchAsync(int empresaId, Dictionary<int, decimal> stockData)
        {
            var options = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = _options.GetTtlTimeSpan(),
                Priority = CacheItemPriority.Normal,
                Size = 1
            };

            foreach (var (productoId, stock) in stockData)
            {
                var key = string.Format(STOCK_KEY_PREFIX, empresaId, productoId);
                _cache.Set(key, stock, options);
            }

            _logger.LogDebug("Batch stock almacenado en caché - Empresa: {EmpresaId}, Productos: {Count}", 
                empresaId, stockData.Count);
        }

        public async Task InvalidateAllStockCacheAsync()
        {
            try
            {
                // Usar un marcador global de invalidación
                _lastInvalidation = DateTime.UtcNow;
                
                var invalidKey = "global:cache:invalidated";
                _cache.Set(invalidKey, _lastInvalidation, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1)
                });
                
                _logger.LogInformation("Caché global marcado como inválido en {Timestamp}", _lastInvalidation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al invalidar todo el caché de stock");
                throw;
            }
        }

        public async Task InvalidateEmpresaCacheAsync(int empresaId)
        {
            try
            {
                var removedCount = 0;

                // Remover caché de productos con stock
                var productosKey = string.Format(PRODUCTOS_CON_STOCK_KEY, empresaId);
                _cache.Remove(productosKey);
                removedCount++;

                // Remover caché warm
                var warmKey = string.Format(CACHE_WARM_KEY, empresaId);
                _cache.Remove(warmKey);
                removedCount++;

                // Usar enfoque simple: marcar como inválido con timestamp
                var invalidKey = string.Format("invalidated:empresa:{0}", empresaId);
                _cache.Set(invalidKey, DateTime.UtcNow, new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = _options.GetTtlTimeSpan()
                });

                _logger.LogInformation("Caché de empresa marcado como inválido - Empresa: {EmpresaId}", empresaId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al invalidar caché de empresa {EmpresaId}", empresaId);
            }
        }

        public async Task<bool> IsCacheWarmAsync(int empresaId)
        {
            var warmKey = string.Format(CACHE_WARM_KEY, empresaId);
            
            if (_cache.TryGetValue(warmKey, out DateTime warmTime))
            {
                var isWarm = DateTime.UtcNow - warmTime < _options.GetTtlTimeSpan();
                _logger.LogDebug("Cache warm check - Empresa: {EmpresaId}, IsWarm: {IsWarm}, WarmTime: {WarmTime}", 
                    empresaId, isWarm, warmTime);
                return isWarm;
            }

            return false;
        }

        public async Task<CacheStatsDto> GetCacheStatsAsync()
        {
            // Contar elementos en caché (aproximado)
            var cachedEmpresasCount = 0;
            var cachedProductosCount = 0;

            try
            {
                if (_cache is MemoryCache concreteCache)
                {
                    var field = typeof(MemoryCache).GetField("_coherentState", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    
                    if (field?.GetValue(concreteCache) is object coherentState)
                    {
                        var entriesCollection = coherentState.GetType()
                            .GetProperty("EntriesCollection", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                        
                        if (entriesCollection?.GetValue(coherentState) is System.Collections.IDictionary entries)
                        {
                            foreach (var key in entries.Keys)
                            {
                                var keyStr = key.ToString();
                                if (keyStr.StartsWith("productos_con_stock:"))
                                    cachedEmpresasCount++;
                                else if (keyStr.StartsWith("stock:"))
                                    cachedProductosCount++;
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Error al obtener estadísticas de caché");
            }

            return new CacheStatsDto
            {
                TotalHits = _totalHits,
                TotalMisses = _totalMisses,
                CachedEmpresasCount = cachedEmpresasCount,
                CachedProductosCount = cachedProductosCount,
                LastInvalidation = _lastInvalidation,
                MemoryUsageBytes = GC.GetTotalMemory(false) // Aproximado
            };
        }
    }

}