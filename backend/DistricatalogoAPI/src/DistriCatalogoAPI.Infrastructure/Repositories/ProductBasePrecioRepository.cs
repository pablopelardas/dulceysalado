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
    public class ProductBasePrecioRepository : IProductBasePrecioRepository
    {
        private readonly DistricatalogoContext _context;
        private readonly ILogger<ProductBasePrecioRepository> _logger;

        public ProductBasePrecioRepository(
            DistricatalogoContext context,
            ILogger<ProductBasePrecioRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task UpsertPreciosAsync(List<ProductoPrecioDto> precios)
        {
            if (!precios.Any()) return;

            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {

                var groupedByLista = precios.GroupBy(p => p.ListaPrecioId).ToList();
                var totalUpdated = 0;
                var totalInserted = 0;

                foreach (var group in groupedByLista)
                {
                    var listaPrecioId = group.Key;
                    var preciosGrupo = group.ToList();
                    var productosIds = preciosGrupo.Select(p => p.ProductoBaseId).ToList();

                    // Obtener precios existentes para esta lista y estos productos
                    var existingPrices = await _context.ProductosBasePrecios
                        .Where(p => p.ListaPrecioId == listaPrecioId && productosIds.Contains(p.ProductoBaseId))
                        .ToDictionaryAsync(p => p.ProductoBaseId, p => p);

                    // Separar updates e inserts
                    var preciosToUpdate = new List<ProductoPrecioDto>();
                    var preciosToInsert = new List<ProductoPrecioDto>();

                    foreach (var precio in preciosGrupo)
                    {
                        if (existingPrices.ContainsKey(precio.ProductoBaseId))
                        {
                            preciosToUpdate.Add(precio);
                        }
                        else
                        {
                            preciosToInsert.Add(precio);
                        }
                    }

                    // Procesar updates
                    if (preciosToUpdate.Any())
                    {
                        foreach (var precio in preciosToUpdate)
                        {
                            var existing = existingPrices[precio.ProductoBaseId];
                            existing.Precio = precio.Precio;
                            existing.UpdatedAt = DateTime.UtcNow;
                            existing.ActualizadoGecom = DateTime.UtcNow;
                        }
                        totalUpdated += preciosToUpdate.Count;
                    }

                    // Procesar inserts en lote
                    if (preciosToInsert.Any())
                    {
                        var newPrecios = preciosToInsert.Select(precio => new ProductosBasePrecio
                        {
                            ProductoBaseId = precio.ProductoBaseId,
                            ListaPrecioId = precio.ListaPrecioId,
                            Precio = precio.Precio,
                            CreatedAt = DateTime.UtcNow,
                            UpdatedAt = DateTime.UtcNow,
                            ActualizadoGecom = DateTime.UtcNow
                        }).ToList();

                        await _context.ProductosBasePrecios.AddRangeAsync(newPrecios);
                        totalInserted += newPrecios.Count;
                        
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Upsert de precios completado: {TotalCount} precios ({Updated} actualizados, {Inserted} insertados)", 
                    precios.Count, totalUpdated, totalInserted);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error en upsert de precios");
                throw;
            }
        }

        public async Task<List<ProductoPrecioConListaDto>> GetPreciosPorProductoAsync(int productoBaseId)
        {
            try
            {
                var precios = await _context.ProductosBasePrecios
                    .Include(p => p.ListaPrecio)
                    .Where(p => p.ProductoBaseId == productoBaseId)
                    .OrderBy(p => p.ListaPrecio.Orden)
                    .ThenBy(p => p.ListaPrecio.Codigo)
                    .Select(p => new ProductoPrecioConListaDto
                    {
                        ProductoBaseId = p.ProductoBaseId,
                        ListaPrecioId = p.ListaPrecioId,
                        ListaPrecioCodigo = p.ListaPrecio.Codigo,
                        ListaPrecioNombre = p.ListaPrecio.Nombre,
                        Precio = p.Precio
                    })
                    .ToListAsync();

                return precios;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo precios para producto {ProductoId}", productoBaseId);
                throw;
            }
        }

        public async Task<List<ProductoPrecioConListaDto>> GetPreciosPorProductoYListaAsync(int productoBaseId, int listaPrecioId)
        {
            try
            {
                var precios = await _context.ProductosBasePrecios
                    .Include(p => p.ListaPrecio)
                    .Where(p => p.ProductoBaseId == productoBaseId && p.ListaPrecioId == listaPrecioId)
                    .Select(p => new ProductoPrecioConListaDto
                    {
                        ProductoBaseId = p.ProductoBaseId,
                        ListaPrecioId = p.ListaPrecioId,
                        ListaPrecioCodigo = p.ListaPrecio.Codigo,
                        ListaPrecioNombre = p.ListaPrecio.Nombre,
                        Precio = p.Precio
                    })
                    .ToListAsync();

                return precios;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo precios para producto {ProductoId} y lista {ListaId}", productoBaseId, listaPrecioId);
                throw;
            }
        }

        public async Task<Dictionary<int, decimal>> GetPreciosPorProductosYListaAsync(List<int> productosBaseIds, int listaPrecioId)
        {
            try
            {
                if (!productosBaseIds.Any())
                    return new Dictionary<int, decimal>();

                var precios = await _context.ProductosBasePrecios
                    .Where(p => productosBaseIds.Contains(p.ProductoBaseId) && p.ListaPrecioId == listaPrecioId)
                    .ToDictionaryAsync(p => p.ProductoBaseId, p => p.Precio);

                return precios;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error obteniendo precios para productos {ProductosIds} en lista {ListaId}", 
                    string.Join(",", productosBaseIds), listaPrecioId);
                throw;
            }
        }

        public async Task<bool> UpsertPrecioAsync(int productoBaseId, int listaPrecioId, decimal precio)
        {
            try
            {
                var existingPrice = await _context.ProductosBasePrecios
                    .FirstOrDefaultAsync(p => p.ProductoBaseId == productoBaseId && p.ListaPrecioId == listaPrecioId);

                if (existingPrice != null)
                {
                    // Actualizar precio existente
                    existingPrice.Precio = precio;
                    existingPrice.UpdatedAt = DateTime.UtcNow;
                    existingPrice.ActualizadoGecom = DateTime.UtcNow;
                }
                else
                {
                    // Crear nuevo precio
                    var newPrecio = new ProductosBasePrecio
                    {
                        ProductoBaseId = productoBaseId,
                        ListaPrecioId = listaPrecioId,
                        Precio = precio,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        ActualizadoGecom = DateTime.UtcNow
                    };
                    _context.ProductosBasePrecios.Add(newPrecio);
                }

                await _context.SaveChangesAsync();
                _logger.LogInformation("Precio upsert exitoso para producto {ProductoId} en lista {ListaId}: {Precio}", 
                    productoBaseId, listaPrecioId, precio);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error en upsert de precio individual para producto {ProductoId} y lista {ListaId}", 
                    productoBaseId, listaPrecioId);
                throw;
            }
        }

        public async Task<bool> DeletePrecioAsync(int productoBaseId, int listaPrecioId)
        {
            try
            {
                var existingPrice = await _context.ProductosBasePrecios
                    .FirstOrDefaultAsync(p => p.ProductoBaseId == productoBaseId && p.ListaPrecioId == listaPrecioId);

                if (existingPrice == null)
                {
                    _logger.LogWarning("Precio no encontrado para eliminar: producto {ProductoId}, lista {ListaId}", 
                        productoBaseId, listaPrecioId);
                    return false;
                }

                _context.ProductosBasePrecios.Remove(existingPrice);
                await _context.SaveChangesAsync();
                
                _logger.LogInformation("Precio eliminado exitosamente para producto {ProductoId} en lista {ListaId}", 
                    productoBaseId, listaPrecioId);
                
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error eliminando precio para producto {ProductoId} y lista {ListaId}", 
                    productoBaseId, listaPrecioId);
                throw;
            }
        }
    }
}