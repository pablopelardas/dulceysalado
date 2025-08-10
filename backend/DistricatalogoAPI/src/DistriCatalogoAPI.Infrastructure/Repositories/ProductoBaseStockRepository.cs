using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Domain.Interfaces;
using DistriCatalogoAPI.Domain.Entities;
using DistriCatalogoAPI.Infrastructure.Models;

namespace DistriCatalogoAPI.Infrastructure.Repositories
{
    public class ProductoBaseStockRepository : IProductoBaseStockRepository
    {
        private readonly DistricatalogoContext _context;
        private readonly ILogger<ProductoBaseStockRepository> _logger;

        public ProductoBaseStockRepository(
            DistricatalogoContext context,
            ILogger<ProductoBaseStockRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<decimal?> GetStockAsync(int empresaId, int productoBaseId)
        {
            try
            {
                var stockRecord = await _context.ProductosBaseStocks
                    .Where(pbs => pbs.EmpresaId == empresaId && pbs.ProductoBaseId == productoBaseId)
                    .FirstOrDefaultAsync();

                return stockRecord?.Existencia;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener stock - Empresa: {EmpresaId}, Producto: {ProductoId}", empresaId, productoBaseId);
                throw;
            }
        }

        public async Task<Dictionary<int, decimal>> GetStockBatchAsync(int empresaId, List<int> productoBaseIds)
        {
            try
            {
                if (!productoBaseIds.Any())
                    return new Dictionary<int, decimal>();

                var result = await _context.ProductosBaseStocks
                    .Where(pbs => pbs.EmpresaId == empresaId && productoBaseIds.Contains(pbs.ProductoBaseId))
                    .ToDictionaryAsync(pbs => pbs.ProductoBaseId, pbs => pbs.Existencia);

                _logger.LogDebug("Batch stock query completado - Empresa: {EmpresaId}, Productos consultados: {Total}, Encontrados: {Found}", 
                    empresaId, productoBaseIds.Count, result.Count);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en consulta batch de stock - Empresa: {EmpresaId}", empresaId);
                throw;
            }
        }

        public async Task<List<int>> GetProductosConStockAsync(int empresaId)
        {
            try
            {
                var productos = await _context.ProductosBaseStocks
                    .Where(pbs => pbs.EmpresaId == empresaId && pbs.Existencia > 0)
                    .Select(pbs => pbs.ProductoBaseId)
                    .ToListAsync();

                _logger.LogDebug("Productos con stock obtenidos - Empresa: {EmpresaId}, Productos: {Count}", 
                    empresaId, productos.Count);

                return productos;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos con stock - Empresa: {EmpresaId}", empresaId);
                throw;
            }
        }

        public async Task<List<ProductoBaseStock>> GetStockByEmpresaAsync(int empresaId)
        {
            try
            {
                var stocks = await _context.ProductosBaseStocks
                    .Where(pbs => pbs.EmpresaId == empresaId)
                    .Include(pbs => pbs.ProductoBase)
                    .ToListAsync();

                return stocks.Select(MapToDomain).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener stock por empresa - Empresa: {EmpresaId}", empresaId);
                throw;
            }
        }

        public async Task<List<ProductoBaseStock>> GetStockByProductoAsync(int productoBaseId)
        {
            try
            {
                var stocks = await _context.ProductosBaseStocks
                    .Where(pbs => pbs.ProductoBaseId == productoBaseId)
                    .Include(pbs => pbs.Empresa)
                    .ToListAsync();

                return stocks.Select(MapToDomain).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener stock por producto - Producto: {ProductoId}", productoBaseId);
                throw;
            }
        }

        public async Task UpdateStockAsync(int empresaId, int productoBaseId, decimal stock)
        {
            try
            {
                var existing = await _context.ProductosBaseStocks
                    .FirstOrDefaultAsync(pbs => pbs.EmpresaId == empresaId && pbs.ProductoBaseId == productoBaseId);

                if (existing != null)
                {
                    existing.Existencia = stock;
                    existing.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    var newStock = new ProductosBaseStock
                    {
                        EmpresaId = empresaId,
                        ProductoBaseId = productoBaseId,
                        Existencia = stock,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.ProductosBaseStocks.Add(newStock);
                }

                await _context.SaveChangesAsync();

                _logger.LogDebug("Stock actualizado - Empresa: {EmpresaId}, Producto: {ProductoId}, Stock: {Stock}", 
                    empresaId, productoBaseId, stock);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar stock - Empresa: {EmpresaId}, Producto: {ProductoId}", 
                    empresaId, productoBaseId);
                throw;
            }
        }

        public async Task UpdateStockForAllEmpresasAsync(int productoBaseId, decimal stock)
        {
            try
            {
                // Obtener todas las empresas activas
                var empresas = await _context.Empresas.Where(e => e.Activa == true).Select(e => e.Id).ToListAsync();

                foreach (var empresaId in empresas)
                {
                    var existing = await _context.ProductosBaseStocks
                        .FirstOrDefaultAsync(pbs => pbs.EmpresaId == empresaId && pbs.ProductoBaseId == productoBaseId);

                    if (existing != null)
                    {
                        existing.Existencia = stock;
                        existing.UpdatedAt = DateTime.UtcNow;
                    }
                    else
                    {
                        var newStock = new ProductosBaseStock
                        {
                            EmpresaId = empresaId,
                            ProductoBaseId = productoBaseId,
                            Existencia = stock,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow
                        };
                        _context.ProductosBaseStocks.Add(newStock);
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Stock actualizado para todas las empresas - Producto: {ProductoId}, Stock: {Stock}, Empresas: {EmpresasCount}", 
                    productoBaseId, stock, empresas.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar stock para todas las empresas - Producto: {ProductoId}", productoBaseId);
                throw;
            }
        }

        public async Task BulkUpdateStockAsync(Dictionary<int, decimal> productosStock, int? empresaId = null)
        {
            try
            {
                if (!productosStock.Any())
                    return;

                var empresas = empresaId.HasValue 
                    ? new List<int> { empresaId.Value }
                    : await _context.Empresas.Where(e => e.Activa == true).Select(e => e.Id).ToListAsync();

                var productoIds = productosStock.Keys.ToList();

                // Obtener registros existentes
                var existingStocks = await _context.ProductosBaseStocks
                    .Where(pbs => empresas.Contains(pbs.EmpresaId) && productoIds.Contains(pbs.ProductoBaseId))
                    .ToListAsync();

                var updatedCount = 0;
                var createdCount = 0;

                foreach (var empresaActual in empresas)
                {
                    foreach (var (productoId, stock) in productosStock)
                    {
                        var existing = existingStocks
                            .FirstOrDefault(es => es.EmpresaId == empresaActual && es.ProductoBaseId == productoId);

                        if (existing != null)
                        {
                            existing.Existencia = stock;
                            existing.UpdatedAt = DateTime.UtcNow;
                            updatedCount++;
                        }
                        else
                        {
                            var newStock = new ProductosBaseStock
                            {
                                EmpresaId = empresaActual,
                                ProductoBaseId = productoId,
                                Existencia = stock,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            };
                            _context.ProductosBaseStocks.Add(newStock);
                            createdCount++;
                        }
                    }
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Bulk update completado - Productos: {ProductosCount}, Empresas: {EmpresasCount}, Updated: {UpdatedCount}, Created: {CreatedCount}", 
                    productosStock.Count, empresas.Count, updatedCount, createdCount);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en bulk update de stock");
                throw;
            }
        }

        public async Task<ProductoBaseStock> CreateAsync(ProductoBaseStock productoBaseStock)
        {
            try
            {
                var infraStock = MapToInfrastructure(productoBaseStock);
                infraStock.CreatedAt = DateTime.UtcNow;
                infraStock.UpdatedAt = DateTime.UtcNow;

                _context.ProductosBaseStocks.Add(infraStock);
                await _context.SaveChangesAsync();

                return MapToDomain(infraStock);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear stock - Empresa: {EmpresaId}, Producto: {ProductoId}", 
                    productoBaseStock.EmpresaId, productoBaseStock.ProductoBaseId);
                throw;
            }
        }

        public async Task UpdateAsync(ProductoBaseStock productoBaseStock)
        {
            try
            {
                var existing = await _context.ProductosBaseStocks
                    .FirstOrDefaultAsync(pbs => pbs.EmpresaId == productoBaseStock.EmpresaId && 
                                               pbs.ProductoBaseId == productoBaseStock.ProductoBaseId);

                if (existing == null)
                {
                    throw new InvalidOperationException($"Stock no encontrado para empresa {productoBaseStock.EmpresaId} y producto {productoBaseStock.ProductoBaseId}");
                }

                existing.Existencia = productoBaseStock.Existencia;
                existing.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar stock - Empresa: {EmpresaId}, Producto: {ProductoId}", 
                    productoBaseStock.EmpresaId, productoBaseStock.ProductoBaseId);
                throw;
            }
        }

        public async Task DeleteAsync(int empresaId, int productoBaseId)
        {
            try
            {
                var stock = await _context.ProductosBaseStocks
                    .FirstOrDefaultAsync(pbs => pbs.EmpresaId == empresaId && pbs.ProductoBaseId == productoBaseId);

                if (stock != null)
                {
                    _context.ProductosBaseStocks.Remove(stock);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar stock - Empresa: {EmpresaId}, Producto: {ProductoId}", 
                    empresaId, productoBaseId);
                throw;
            }
        }

        public async Task DeleteByProductoAsync(int productoBaseId)
        {
            try
            {
                var stocks = await _context.ProductosBaseStocks
                    .Where(pbs => pbs.ProductoBaseId == productoBaseId)
                    .ToListAsync();

                if (stocks.Any())
                {
                    _context.ProductosBaseStocks.RemoveRange(stocks);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Stock eliminado para producto - ProductoId: {ProductoId}, Registros: {Count}", 
                        productoBaseId, stocks.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar stock por producto - ProductoId: {ProductoId}", productoBaseId);
                throw;
            }
        }

        public async Task DeleteByEmpresaAsync(int empresaId)
        {
            try
            {
                var stocks = await _context.ProductosBaseStocks
                    .Where(pbs => pbs.EmpresaId == empresaId)
                    .ToListAsync();

                if (stocks.Any())
                {
                    _context.ProductosBaseStocks.RemoveRange(stocks);
                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Stock eliminado para empresa - EmpresaId: {EmpresaId}, Registros: {Count}", 
                        empresaId, stocks.Count);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar stock por empresa - EmpresaId: {EmpresaId}", empresaId);
                throw;
            }
        }

        public async Task<bool> ExistsAsync(int empresaId, int productoBaseId)
        {
            try
            {
                return await _context.ProductosBaseStocks
                    .AnyAsync(pbs => pbs.EmpresaId == empresaId && pbs.ProductoBaseId == productoBaseId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de stock - Empresa: {EmpresaId}, Producto: {ProductoId}", 
                    empresaId, productoBaseId);
                throw;
            }
        }

        public async Task<StockStatsDto> GetStockStatsAsync(int? empresaId = null)
        {
            try
            {
                var query = _context.ProductosBaseStocks.AsQueryable();

                if (empresaId.HasValue)
                {
                    query = query.Where(pbs => pbs.EmpresaId == empresaId.Value);
                }

                var stats = await query
                    .GroupBy(pbs => 1)
                    .Select(g => new StockStatsDto
                    {
                        TotalProductos = g.Count(),
                        ProductosConStock = g.Count(pbs => pbs.Existencia > 0),
                        ProductosSinStock = g.Count(pbs => pbs.Existencia <= 0),
                        StockTotal = g.Sum(pbs => pbs.Existencia),
                        StockPromedio = g.Average(pbs => pbs.Existencia),
                        EmpresasConStock = g.Select(pbs => pbs.EmpresaId).Distinct().Count()
                    })
                    .FirstOrDefaultAsync();

                return stats ?? new StockStatsDto();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener estadísticas de stock");
                throw;
            }
        }

        public async Task<Dictionary<int, decimal>> GetAllStockForEmpresaAsync(int empresaId)
        {
            try
            {
                _logger.LogDebug("Obteniendo todo el stock para empresa {EmpresaId}", empresaId);

                var stockData = await _context.ProductosBaseStocks
                    .Where(pbs => pbs.EmpresaId == empresaId)
                    .ToDictionaryAsync(pbs => pbs.ProductoBaseId, pbs => pbs.Existencia);

                _logger.LogDebug("Obtenido stock de {ProductCount} productos para empresa {EmpresaId}", 
                    stockData.Count, empresaId);

                return stockData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todo el stock para empresa {EmpresaId}", empresaId);
                throw;
            }
        }

        // Métodos de mapeo
        private static Domain.Entities.ProductoBaseStock MapToDomain(ProductosBaseStock infraStock)
        {
            var domainStock = new Domain.Entities.ProductoBaseStock
            {
                Id = infraStock.Id,
                EmpresaId = infraStock.EmpresaId,
                ProductoBaseId = infraStock.ProductoBaseId,
                Existencia = infraStock.Existencia
            };
            
            // Configurar fechas usando métodos protegidos
            domainStock.SetCreatedAt(infraStock.CreatedAt);
            domainStock.SetUpdatedAt(infraStock.UpdatedAt);
            
            return domainStock;
        }

        private static ProductosBaseStock MapToInfrastructure(Domain.Entities.ProductoBaseStock domainStock)
        {
            return new ProductosBaseStock
            {
                Id = domainStock.Id,
                EmpresaId = domainStock.EmpresaId,
                ProductoBaseId = domainStock.ProductoBaseId,
                Existencia = domainStock.Existencia,
                CreatedAt = domainStock.CreatedAt,
                UpdatedAt = domainStock.UpdatedAt
            };
        }
    }
}