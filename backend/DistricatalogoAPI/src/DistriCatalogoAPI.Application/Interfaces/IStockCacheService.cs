using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DistriCatalogoAPI.Application.Interfaces
{
    /// <summary>
    /// Servicio de caché para gestión de stock por empresa
    /// Optimizado para sincronización diaria con invalidación inteligente
    /// </summary>
    public interface IStockCacheService
    {
        /// <summary>
        /// Obtiene el stock de un producto para una empresa específica desde caché
        /// </summary>
        /// <param name="empresaId">ID de la empresa</param>
        /// <param name="productoId">ID del producto base</param>
        /// <returns>Stock del producto o null si no está en caché</returns>
        Task<decimal?> GetStockAsync(int empresaId, int productoId);

        /// <summary>
        /// Almacena el stock de un producto para una empresa en caché
        /// </summary>
        /// <param name="empresaId">ID de la empresa</param>
        /// <param name="productoId">ID del producto base</param>
        /// <param name="stock">Cantidad en stock</param>
        Task SetStockAsync(int empresaId, int productoId, decimal stock);

        /// <summary>
        /// Obtiene los IDs de productos que tienen stock > 0 para una empresa
        /// </summary>
        /// <param name="empresaId">ID de la empresa</param>
        /// <returns>Lista de IDs de productos con stock</returns>
        Task<List<int>> GetProductosConStockAsync(int empresaId);

        /// <summary>
        /// Almacena la lista de productos con stock para una empresa
        /// </summary>
        /// <param name="empresaId">ID de la empresa</param>
        /// <param name="productoIds">Lista de IDs de productos con stock</param>
        Task SetProductosConStockAsync(int empresaId, List<int> productoIds);

        /// <summary>
        /// Obtiene stock de múltiples productos para una empresa desde caché
        /// </summary>
        /// <param name="empresaId">ID de la empresa</param>
        /// <param name="productoIds">Lista de IDs de productos</param>
        /// <returns>Diccionario con stock por producto (solo productos encontrados en caché)</returns>
        Task<Dictionary<int, decimal>> GetStockBatchAsync(int empresaId, List<int> productoIds);

        /// <summary>
        /// Almacena stock de múltiples productos para una empresa
        /// </summary>
        /// <param name="empresaId">ID de la empresa</param>
        /// <param name="stockData">Diccionario con stock por producto</param>
        Task SetStockBatchAsync(int empresaId, Dictionary<int, decimal> stockData);

        /// <summary>
        /// Invalida todo el caché de stock (usado durante sincronización)
        /// </summary>
        Task InvalidateAllStockCacheAsync();

        /// <summary>
        /// Invalida el caché de stock de una empresa específica
        /// </summary>
        /// <param name="empresaId">ID de la empresa</param>
        Task InvalidateEmpresaCacheAsync(int empresaId);

        /// <summary>
        /// Verifica si el caché está "caliente" para una empresa
        /// </summary>
        /// <param name="empresaId">ID de la empresa</param>
        /// <returns>True si el caché contiene datos recientes</returns>
        Task<bool> IsCacheWarmAsync(int empresaId);

        /// <summary>
        /// Obtiene estadísticas del caché (para monitoreo)
        /// </summary>
        /// <returns>Información sobre hit/miss ratio y uso de memoria</returns>
        Task<CacheStatsDto> GetCacheStatsAsync();
    }

    /// <summary>
    /// DTO para estadísticas del caché
    /// </summary>
    public class CacheStatsDto
    {
        public long TotalHits { get; set; }
        public long TotalMisses { get; set; }
        public double HitRatio => TotalHits + TotalMisses > 0 ? (double)TotalHits / (TotalHits + TotalMisses) : 0;
        public int CachedEmpresasCount { get; set; }
        public int CachedProductosCount { get; set; }
        public DateTime LastInvalidation { get; set; }
        public long MemoryUsageBytes { get; set; }
    }
}