using System.Collections.Generic;
using System.Threading.Tasks;
using DistriCatalogoAPI.Domain.Entities;

namespace DistriCatalogoAPI.Domain.Interfaces
{
    /// <summary>
    /// Repositorio para gestión de stock de productos base por empresa
    /// Integrado con sistema de caché para optimización de performance
    /// </summary>
    public interface IProductoBaseStockRepository
    {
        /// <summary>
        /// Obtiene el stock de un producto específico para una empresa
        /// Utiliza caché primero, BD como fallback
        /// </summary>
        /// <param name="empresaId">ID de la empresa</param>
        /// <param name="productoBaseId">ID del producto base</param>
        /// <returns>Cantidad en stock o null si no existe</returns>
        Task<decimal?> GetStockAsync(int empresaId, int productoBaseId);

        /// <summary>
        /// Obtiene el stock de múltiples productos para una empresa
        /// Optimizado para consultas batch con caché
        /// </summary>
        /// <param name="empresaId">ID de la empresa</param>
        /// <param name="productoBaseIds">Lista de IDs de productos</param>
        /// <returns>Diccionario con stock por producto</returns>
        Task<Dictionary<int, decimal>> GetStockBatchAsync(int empresaId, List<int> productoBaseIds);

        /// <summary>
        /// Obtiene la lista de IDs de productos que tienen stock > 0 para una empresa
        /// Utilizado para filtros de "solo con stock"
        /// </summary>
        /// <param name="empresaId">ID de la empresa</param>
        /// <returns>Lista de IDs de productos con stock</returns>
        Task<List<int>> GetProductosConStockAsync(int empresaId);

        /// <summary>
        /// Obtiene todos los registros de stock para una empresa
        /// </summary>
        /// <param name="empresaId">ID de la empresa</param>
        /// <returns>Lista de registros de stock</returns>
        Task<List<ProductoBaseStock>> GetStockByEmpresaAsync(int empresaId);

        /// <summary>
        /// Obtiene todos los registros de stock para un producto en todas las empresas
        /// </summary>
        /// <param name="productoBaseId">ID del producto base</param>
        /// <returns>Lista de registros de stock por empresa</returns>
        Task<List<ProductoBaseStock>> GetStockByProductoAsync(int productoBaseId);

        /// <summary>
        /// Actualiza el stock de un producto para una empresa específica
        /// Invalida caché correspondiente
        /// </summary>
        /// <param name="empresaId">ID de la empresa</param>
        /// <param name="productoBaseId">ID del producto base</param>
        /// <param name="stock">Nueva cantidad</param>
        Task UpdateStockAsync(int empresaId, int productoBaseId, decimal stock);

        /// <summary>
        /// Actualiza el stock de un producto para TODAS las empresas
        /// Utilizado durante sincronización
        /// </summary>
        /// <param name="productoBaseId">ID del producto base</param>
        /// <param name="stock">Nueva cantidad para todas las empresas</param>
        Task UpdateStockForAllEmpresasAsync(int productoBaseId, decimal stock);

        /// <summary>
        /// Actualización masiva de stock para múltiples productos
        /// Optimizado para sincronización diaria
        /// </summary>
        /// <param name="productosStock">Diccionario producto_id -> stock</param>
        /// <param name="empresaId">ID de empresa específica o null para todas</param>
        Task BulkUpdateStockAsync(Dictionary<int, decimal> productosStock, int? empresaId = null);

        /// <summary>
        /// Crea un nuevo registro de stock
        /// </summary>
        /// <param name="productoBaseStock">Entidad de stock a crear</param>
        /// <returns>Entidad creada con ID asignado</returns>
        Task<ProductoBaseStock> CreateAsync(ProductoBaseStock productoBaseStock);

        /// <summary>
        /// Actualiza un registro de stock existente
        /// </summary>
        /// <param name="productoBaseStock">Entidad de stock a actualizar</param>
        Task UpdateAsync(ProductoBaseStock productoBaseStock);

        /// <summary>
        /// Elimina un registro de stock
        /// </summary>
        /// <param name="empresaId">ID de la empresa</param>
        /// <param name="productoBaseId">ID del producto base</param>
        Task DeleteAsync(int empresaId, int productoBaseId);

        /// <summary>
        /// Elimina todos los registros de stock de un producto
        /// Utilizado cuando se elimina un producto base
        /// </summary>
        /// <param name="productoBaseId">ID del producto base</param>
        Task DeleteByProductoAsync(int productoBaseId);

        /// <summary>
        /// Elimina todos los registros de stock de una empresa
        /// Utilizado cuando se elimina una empresa
        /// </summary>
        /// <param name="empresaId">ID de la empresa</param>
        Task DeleteByEmpresaAsync(int empresaId);

        /// <summary>
        /// Verifica si existe un registro de stock
        /// </summary>
        /// <param name="empresaId">ID de la empresa</param>
        /// <param name="productoBaseId">ID del producto base</param>
        /// <returns>True si existe el registro</returns>
        Task<bool> ExistsAsync(int empresaId, int productoBaseId);

        /// <summary>
        /// Obtiene todo el stock de una empresa como diccionario producto_id -> stock
        /// Utilizado para precalentamiento de caché
        /// </summary>
        /// <param name="empresaId">ID de la empresa</param>
        /// <returns>Diccionario con todo el stock de la empresa</returns>
        Task<Dictionary<int, decimal>> GetAllStockForEmpresaAsync(int empresaId);

        /// <summary>
        /// Obtiene estadísticas de stock para monitoreo
        /// </summary>
        /// <param name="empresaId">ID de empresa específica o null para todas</param>
        /// <returns>Estadísticas de stock</returns>
        Task<StockStatsDto> GetStockStatsAsync(int? empresaId = null);
    }

    /// <summary>
    /// DTO para estadísticas de stock
    /// </summary>
    public class StockStatsDto
    {
        public int TotalProductos { get; set; }
        public int ProductosConStock { get; set; }
        public int ProductosSinStock { get; set; }
        public decimal StockTotal { get; set; }
        public decimal StockPromedio { get; set; }
        public int EmpresasConStock { get; set; }
        public Dictionary<int, int> ProductosPorEmpresa { get; set; } = new();
    }
}