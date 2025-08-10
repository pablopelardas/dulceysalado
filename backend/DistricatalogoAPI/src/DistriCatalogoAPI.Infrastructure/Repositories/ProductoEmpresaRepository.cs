using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Domain.Entities;
using DistriCatalogoAPI.Domain.Interfaces;
using DistriCatalogoAPI.Infrastructure.Models;

namespace DistriCatalogoAPI.Infrastructure.Repositories
{
    public class ProductoEmpresaRepository : IProductoEmpresaRepository
    {
        private readonly DistricatalogoContext _context;
        private readonly ILogger<ProductoEmpresaRepository> _logger;

        public ProductoEmpresaRepository(
            DistricatalogoContext context,
            ILogger<ProductoEmpresaRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ProductoEmpresa?> GetByIdAsync(int id)
        {
            try
            {
                var productModel = await _context.ProductosEmpresas
                    .Include(p => p.Empresa)
                    .FirstOrDefaultAsync(p => p.Id == id);

                return productModel != null ? MapToDomain(productModel) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener producto empresa por ID {Id}", id);
                throw;
            }
        }

        public async Task<(List<ProductoEmpresa> products, int total)> GetPagedByEmpresaAsync(
            int? empresaId,
            bool? visible,
            bool? destacado,
            int? codigoRubro,
            string? busqueda,
            int page,
            int pageSize,
            string? sortBy = null,
            string? sortOrder = null,
            int? listaPrecioIdForSorting = null,
            bool incluirSinExistencia = false)
        {
            try
            {
                var query = _context.ProductosEmpresas.AsQueryable();
                
                // Filtrar por empresa solo si se especifica
                if (empresaId.HasValue)
                {
                    query = query.Where(p => p.EmpresaId == empresaId.Value);
                }

                if (visible.HasValue)
                    query = query.Where(p => p.Visible == visible.Value);

                if (destacado.HasValue)
                    query = query.Where(p => p.Destacado == destacado.Value);

                if (codigoRubro.HasValue)
                    query = query.Where(p => p.CodigoRubro == codigoRubro.Value);

                if (!string.IsNullOrEmpty(busqueda))
                    query = query.Where(p => p.Descripcion.Contains(busqueda) || 
                                           (p.DescripcionCorta != null && p.DescripcionCorta.Contains(busqueda)) ||
                                           (p.Tags != null && p.Tags.Contains(busqueda)));

                // Filtro de existencia (por defecto solo con existencia > 0)
                if (!incluirSinExistencia)
                    query = query.Where(p => p.Existencia > 0);

                var total = await query.CountAsync();
                
                // Aplicar ordenamiento
                if (!string.IsNullOrEmpty(sortBy))
                {
                    var isDescending = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);
                    
                    // Si se ordena por precio, necesitamos hacer JOIN con tabla de precios
                    if (sortBy.ToLower() == "precio" && listaPrecioIdForSorting.HasValue)
                    {
                        var queryWithPrices = from p in query
                                            join precio in _context.ProductosEmpresaPrecios
                                                on new { ProductoId = p.Id, ListaId = listaPrecioIdForSorting.Value }
                                                equals new { ProductoId = precio.ProductoId, ListaId = precio.ListaPrecioId }
                                                into precios
                                            from precio in precios.DefaultIfEmpty()
                                            where precio == null || precio.TipoProducto == "empresa"
                                            select new { Product = p, Precio = precio != null ? precio.PrecioOverride ?? 0 : 0 };

                        var orderedWithPrices = isDescending ? 
                            queryWithPrices.OrderByDescending(x => x.Precio) : 
                            queryWithPrices.OrderBy(x => x.Precio);

                        var productsWithPrices = await orderedWithPrices
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .Select(x => x.Product)
                            .Include(p => p.Empresa)
                            .ToListAsync();

                        return (productsWithPrices.Select(MapToDomain).ToList(), total);
                    }
                    else
                    {
                        // Ordenamiento normal (sin precios)
                        query = sortBy.ToLower() switch
                        {
                            "descripcion" => isDescending ? 
                                query.OrderByDescending(p => p.Descripcion) : 
                                query.OrderBy(p => p.Descripcion),
                            "codigo" => isDescending ? 
                                query.OrderByDescending(p => p.Codigo) : 
                                query.OrderBy(p => p.Codigo),
                            "codigorubro" => isDescending ? 
                                query.OrderByDescending(p => p.CodigoRubro) : 
                                query.OrderBy(p => p.CodigoRubro),
                            "existencia" or "stock" => isDescending ? 
                                query.OrderByDescending(p => p.Existencia) : 
                                query.OrderBy(p => p.Existencia),
                            "visible" => isDescending ? 
                                query.OrderByDescending(p => p.Visible) : 
                                query.OrderBy(p => p.Visible),
                            "destacado" => isDescending ? 
                                query.OrderByDescending(p => p.Destacado) : 
                                query.OrderBy(p => p.Destacado),
                            "precio" => query.OrderBy(p => p.Id), // Si no hay lista, usar ordenamiento por defecto
                            _ => query.OrderBy(p => p.Id) // Default
                        };
                    }
                }
                else
                {
                    // Ordenamiento por defecto
                    query = query.OrderBy(p => p.Id);
                }
                
                var products = await query
                    .Include(p => p.Empresa)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
                    
                var domainProducts = products.Select(MapToDomain).ToList();

                return (domainProducts, total);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos empresa paginados para empresa {EmpresaId}", empresaId?.ToString() ?? "TODAS");
                throw;
            }
        }

        public async Task<ProductoEmpresa> CreateAsync(ProductoEmpresa product)
        {
            try
            {
                var productModel = MapToInfrastructure(product);
                
                _context.ProductosEmpresas.Add(productModel);
                await _context.SaveChangesAsync();

                _logger.LogDebug("Producto empresa creado: Id={Id}, Codigo={Codigo}, EmpresaId={EmpresaId}", 
                    productModel.Id, productModel.Codigo, productModel.EmpresaId);
                
                return MapToDomain(productModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear producto empresa - Codigo: {Codigo}, EmpresaId: {EmpresaId}", 
                    product.Codigo, product.EmpresaId);
                throw;
            }
        }

        public async Task UpdateAsync(ProductoEmpresa product)
        {
            try
            {
                var existingModel = await _context.ProductosEmpresas
                    .FirstOrDefaultAsync(p => p.Id == product.Id);

                if (existingModel == null)
                {
                    throw new InvalidOperationException($"Producto empresa {product.Id} no encontrado para actualizar");
                }

                // Actualizar campos del modelo
                existingModel.Descripcion = product.Descripcion;
                existingModel.CodigoRubro = product.CodigoRubro;
                // existingModel.Precio = product.Precio; // ELIMINADO: precio ahora está en productos_empresa_precios
                existingModel.Existencia = product.Existencia;
                existingModel.Visible = product.Visible;
                existingModel.Destacado = product.Destacado;
                existingModel.OrdenCategoria = product.OrdenCategoria;
                existingModel.ImagenUrl = product.ImagenUrl;
                existingModel.ImagenAlt = product.ImagenAlt;
                existingModel.DescripcionCorta = product.DescripcionCorta;
                existingModel.DescripcionLarga = product.DescripcionLarga;
                existingModel.Tags = product.Tags;
                existingModel.CodigoBarras = product.CodigoBarras;
                existingModel.Marca = product.Marca;
                existingModel.UnidadMedida = product.UnidadMedida;
                existingModel.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogDebug("Producto empresa actualizado: Id={Id}, Codigo={Codigo}, EmpresaId={EmpresaId}", 
                    product.Id, product.Codigo, product.EmpresaId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar producto empresa {Id}", product.Id);
                throw;
            }
        }

        public async Task DeleteAsync(ProductoEmpresa product)
        {
            try
            {
                var productModel = await _context.ProductosEmpresas
                    .FirstOrDefaultAsync(p => p.Id == product.Id);

                if (productModel != null)
                {
                    _context.ProductosEmpresas.Remove(productModel);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogDebug("Producto empresa eliminado: Id={Id}, Codigo={Codigo}, EmpresaId={EmpresaId}", 
                        productModel.Id, productModel.Codigo, productModel.EmpresaId);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar producto empresa {Id}", product.Id);
                throw;
            }
        }

        public async Task<bool> ExistsByCodigoAsync(string codigo, int empresaId)
        {
            try
            {
                return await _context.ProductosEmpresas
                    .AnyAsync(p => p.Codigo == codigo && p.EmpresaId == empresaId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de producto empresa - Codigo: {Codigo}, EmpresaId: {EmpresaId}", 
                    codigo, empresaId);
                throw;
            }
        }

        public async Task<bool> ExistsByCodigoInPrincipalCompanyAsync(string codigo, int empresaPrincipalId)
        {
            try
            {
                return await _context.ProductosEmpresas
                    .Join(_context.Empresas, pe => pe.EmpresaId, e => e.Id, (pe, e) => new { pe, e })
                    .AnyAsync(joined => joined.pe.Codigo == codigo && 
                                      (joined.e.Id == empresaPrincipalId || joined.e.EmpresaPrincipalId == empresaPrincipalId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de producto empresa {Codigo} en empresa principal {EmpresaPrincipalId}", codigo, empresaPrincipalId);
                throw;
            }
        }

        private ProductoEmpresa MapToDomain(ProductosEmpresa model)
        {
            // Usar reflection para crear instancia y establecer propiedades privadas
            var product = Activator.CreateInstance(typeof(ProductoEmpresa), true) as ProductoEmpresa;
            var productType = typeof(ProductoEmpresa);

            _logger.LogDebug("Mapeando ProductoEmpresa desde DB: Id={Id}, Codigo={Codigo}, EmpresaId={EmpresaId}", 
                model.Id, model.Codigo, model.EmpresaId);

            void SetProperty(string propertyName, object value)
            {
                var prop = productType.GetProperty(propertyName);
                if (prop != null && prop.CanWrite)
                {
                    prop.SetValue(product, value);
                }
                else
                {
                    var field = productType.GetField($"<{propertyName}>k__BackingField", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (field != null)
                    {
                        field.SetValue(product, value);
                    }
                    else
                    {
                        _logger.LogDebug("No se pudo establecer propiedad {PropertyName} en ProductoEmpresa", propertyName);
                    }
                }
            }

            SetProperty("Id", model.Id);
            SetProperty("EmpresaId", model.EmpresaId);
            SetProperty("Codigo", model.Codigo);
            SetProperty("Descripcion", model.Descripcion ?? "");
            SetProperty("CodigoRubro", model.CodigoRubro);
            // SetProperty("Precio", model.Precio); // ELIMINADO: precio ahora está en productos_empresa_precios
            SetProperty("Existencia", model.Existencia);
            SetProperty("Visible", model.Visible);
            SetProperty("Destacado", model.Destacado);
            SetProperty("OrdenCategoria", model.OrdenCategoria);
            SetProperty("ImagenUrl", model.ImagenUrl);
            SetProperty("ImagenAlt", model.ImagenAlt);
            SetProperty("DescripcionCorta", model.DescripcionCorta);
            SetProperty("DescripcionLarga", model.DescripcionLarga);
            SetProperty("Tags", model.Tags);
            SetProperty("CodigoBarras", model.CodigoBarras);
            SetProperty("Marca", model.Marca);
            SetProperty("UnidadMedida", model.UnidadMedida ?? "UN");
            SetProperty("CreatedAt", model.CreatedAt ?? DateTime.UtcNow);
            SetProperty("UpdatedAt", model.UpdatedAt ?? DateTime.UtcNow);

            _logger.LogDebug("ProductoEmpresa mapeado exitosamente: Id={Id}, Codigo={Codigo}, EmpresaId={EmpresaId}", 
                product?.Id, product?.Codigo, product?.EmpresaId);

            return product;
        }

        private ProductosEmpresa MapToInfrastructure(ProductoEmpresa domain)
        {
            return new ProductosEmpresa
            {
                EmpresaId = domain.EmpresaId,
                Codigo = domain.Codigo,
                Descripcion = domain.Descripcion,
                CodigoRubro = domain.CodigoRubro,
                // Precio = domain.Precio, // ELIMINADO: precio ahora está en productos_empresa_precios
                Existencia = domain.Existencia,
                Visible = domain.Visible,
                Destacado = domain.Destacado,
                OrdenCategoria = domain.OrdenCategoria,
                ImagenUrl = domain.ImagenUrl,
                ImagenAlt = domain.ImagenAlt,
                DescripcionCorta = domain.DescripcionCorta,
                DescripcionLarga = domain.DescripcionLarga,
                Tags = domain.Tags,
                CodigoBarras = domain.CodigoBarras,
                Marca = domain.Marca,
                UnidadMedida = domain.UnidadMedida,
                CreatedAt = domain.CreatedAt,
                UpdatedAt = domain.UpdatedAt
            };
        }
    }
}