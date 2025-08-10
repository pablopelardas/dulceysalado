using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Domain.Interfaces;
using DistriCatalogoAPI.Infrastructure.Models;

namespace DistriCatalogoAPI.Infrastructure.Repositories
{
    public class ProductoEmpresaPrecioRepository : IProductoEmpresaPrecioRepository
    {
        private readonly DistricatalogoContext _context;
        private readonly ILogger<ProductoEmpresaPrecioRepository> _logger;

        public ProductoEmpresaPrecioRepository(
            DistricatalogoContext context,
            ILogger<ProductoEmpresaPrecioRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task UpsertPreciosAsync(List<ProductoEmpresaPrecioDto> precios)
        {
            if (!precios.Any()) return;

            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                _logger.LogDebug("Iniciando upsert de {Count} precios empresa", precios.Count);

                var groupedByLista = precios.GroupBy(p => p.ListaPrecioId).ToList();

                foreach (var group in groupedByLista)
                {
                    var listaPrecioId = group.Key;
                    var preciosGrupo = group.ToList();
                    var productosIds = preciosGrupo.Select(p => p.ProductoEmpresaId).ToList();

                    // Obtener precios existentes para esta lista y estos productos
                    var existingPrices = await _context.ProductosEmpresaPrecios
                        .Where(p => p.ListaPrecioId == listaPrecioId && productosIds.Contains(p.ProductoId) && p.TipoProducto == "empresa")
                        .ToDictionaryAsync(p => p.ProductoId, p => p);

                    foreach (var precio in preciosGrupo)
                    {
                        if (existingPrices.TryGetValue(precio.ProductoEmpresaId, out var existing))
                        {
                            // Actualizar precio existente
                            existing.PrecioOverride = precio.Precio;
                            existing.UpdatedAt = DateTime.UtcNow;
                        }
                        else
                        {
                            // Crear nuevo precio
                            var newPrecio = new ProductosEmpresaPrecio
                            {
                                EmpresaId = 0, // Esto debería venir del contexto
                                ProductoId = precio.ProductoEmpresaId,
                                TipoProducto = "empresa",
                                ListaPrecioId = precio.ListaPrecioId,
                                PrecioOverride = precio.Precio,
                                CreatedAt = DateTime.UtcNow,
                                UpdatedAt = DateTime.UtcNow
                            };
                            _context.ProductosEmpresaPrecios.Add(newPrecio);
                        }
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Upsert de precios empresa completado: {Count} precios procesados", precios.Count);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error en upsert de precios empresa");
                throw;
            }
        }

        public async Task<List<ProductoEmpresaPrecioConListaDto>> GetPreciosPorProductoAsync(int productoEmpresaId)
        {
            try
            {
                var precios = await _context.ProductosEmpresaPrecios
                    .Include(p => p.ListaPrecio)
                    .Where(p => p.ProductoId == productoEmpresaId && p.TipoProducto == "empresa")
                    .OrderBy(p => p.ListaPrecio.Orden)
                    .ThenBy(p => p.ListaPrecio.Codigo)
                    .Select(p => new ProductoEmpresaPrecioConListaDto
                    {
                        ProductoEmpresaId = p.ProductoId,
                        ListaPrecioId = p.ListaPrecioId,
                        ListaPrecioCodigo = p.ListaPrecio.Codigo,
                        ListaPrecioNombre = p.ListaPrecio.Nombre,
                        Precio = p.PrecioOverride ?? 0
                    })
                    .ToListAsync();

                return precios;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo precios para producto empresa {ProductoId}", productoEmpresaId);
                throw;
            }
        }

        public async Task<List<ProductoEmpresaPrecioConListaDto>> GetPreciosPorProductoYListaAsync(int productoEmpresaId, int listaPrecioId)
        {
            try
            {
                var precios = await _context.ProductosEmpresaPrecios
                    .Include(p => p.ListaPrecio)
                    .Where(p => p.ProductoId == productoEmpresaId && p.ListaPrecioId == listaPrecioId && p.TipoProducto == "empresa")
                    .Select(p => new ProductoEmpresaPrecioConListaDto
                    {
                        ProductoEmpresaId = p.ProductoId,
                        ListaPrecioId = p.ListaPrecioId,
                        ListaPrecioCodigo = p.ListaPrecio.Codigo,
                        ListaPrecioNombre = p.ListaPrecio.Nombre,
                        Precio = p.PrecioOverride ?? 0
                    })
                    .ToListAsync();

                return precios;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo precios para producto empresa {ProductoId} y lista {ListaId}", productoEmpresaId, listaPrecioId);
                throw;
            }
        }

        public async Task<Dictionary<int, decimal?>> GetPreciosPorProductosYListaAsync(List<int> productosEmpresaIds, int listaPrecioId)
        {
            try
            {
                if (!productosEmpresaIds.Any())
                    return new Dictionary<int, decimal?>();

                var precios = await _context.ProductosEmpresaPrecios
                    .Where(p => productosEmpresaIds.Contains(p.ProductoId) && p.ListaPrecioId == listaPrecioId && p.TipoProducto == "empresa")
                    .ToDictionaryAsync(p => p.ProductoId, p => p.PrecioOverride);

                return precios;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo precios para productos empresa {ProductosIds} en lista {ListaId}", 
                    string.Join(",", productosEmpresaIds), listaPrecioId);
                throw;
            }
        }

        public async Task<bool> UpsertPrecioAsync(int productoEmpresaId, int listaPrecioId, decimal precio)
        {
            try
            {
                var existingPrice = await _context.ProductosEmpresaPrecios
                    .FirstOrDefaultAsync(p => p.ProductoId == productoEmpresaId && p.ListaPrecioId == listaPrecioId && p.TipoProducto == "empresa");

                if (existingPrice != null)
                {
                    // Actualizar precio existente
                    existingPrice.PrecioOverride = precio;
                    existingPrice.UpdatedAt = DateTime.UtcNow;
                }
                else
                {
                    // Obtener EmpresaId del producto empresa
                    var productoEmpresa = await _context.ProductosEmpresas
                        .FirstOrDefaultAsync(p => p.Id == productoEmpresaId);
                    
                    if (productoEmpresa == null)
                    {
                        throw new InvalidOperationException($"Producto empresa {productoEmpresaId} no encontrado");
                    }
                    
                    // Crear nuevo precio
                    var newPrecio = new ProductosEmpresaPrecio
                    {
                        EmpresaId = productoEmpresa.EmpresaId,
                        ProductoId = productoEmpresaId,
                        TipoProducto = "empresa",
                        ListaPrecioId = listaPrecioId,
                        PrecioOverride = precio,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    _context.ProductosEmpresaPrecios.Add(newPrecio);
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Precio upsert exitoso para producto empresa {ProductoId} en lista {ListaId}: {Precio}", 
                    productoEmpresaId, listaPrecioId, precio);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en upsert de precio individual para producto empresa {ProductoId} y lista {ListaId}", 
                    productoEmpresaId, listaPrecioId);
                throw;
            }
        }

        public async Task<bool> DeletePrecioAsync(int productoEmpresaId, int listaPrecioId)
        {
            try
            {
                var existingPrice = await _context.ProductosEmpresaPrecios
                    .FirstOrDefaultAsync(p => p.ProductoId == productoEmpresaId && p.ListaPrecioId == listaPrecioId && p.TipoProducto == "empresa");

                if (existingPrice == null)
                {
                    _logger.LogWarning("Precio no encontrado para eliminar: producto empresa {ProductoId}, lista {ListaId}", 
                        productoEmpresaId, listaPrecioId);
                    return false;
                }

                _context.ProductosEmpresaPrecios.Remove(existingPrice);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Precio eliminado exitosamente para producto empresa {ProductoId} en lista {ListaId}", 
                    productoEmpresaId, listaPrecioId);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando precio para producto empresa {ProductoId} y lista {ListaId}", 
                    productoEmpresaId, listaPrecioId);
                throw;
            }
        }
    }
}