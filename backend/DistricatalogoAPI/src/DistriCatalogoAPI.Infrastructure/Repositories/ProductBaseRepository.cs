using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Domain.Entities;
using DistriCatalogoAPI.Domain.Interfaces;
using DistriCatalogoAPI.Infrastructure.Models;
using AutoMapper;

namespace DistriCatalogoAPI.Infrastructure.Repositories
{
    public class ProductBaseRepository : IProductBaseRepository
    {
        private readonly DistricatalogoContext _context;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductBaseRepository> _logger;

        public ProductBaseRepository(
            DistricatalogoContext context,
            IMapper mapper,
            ILogger<ProductBaseRepository> logger)
        {
            _context = context;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<ProductBase?> GetByIdAsync(int id)
        {
            try
            {
                var productModel = await _context.ProductosBases
                    .Include(p => p.AdministradoPorEmpresa)
                    .FirstOrDefaultAsync(p => p.Id == id);

                return productModel != null ? MapToDomain(productModel) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener producto por ID {Id}", id);
                throw;
            }
        }

        public async Task<ProductBase?> GetByCodigoAsync(string codigo)
        {
            try
            {
                var productModel = await _context.ProductosBases
                    .Include(p => p.AdministradoPorEmpresa)
                    .FirstOrDefaultAsync(p => p.Codigo == codigo);

                return productModel != null ? MapToDomain(productModel) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener producto por código {Codigo}", codigo);
                throw;
            }
        }

        public async Task<ProductBase?> GetByCodigoAsync(string codigo, int empresaId)
        {
            try
            {
                var productModel = await _context.ProductosBases
                    .FirstOrDefaultAsync(p => p.Codigo == codigo && 
                                            p.AdministradoPorEmpresaId == empresaId);

                return productModel != null ? MapToDomain(productModel) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener producto por código {Codigo} y empresa {EmpresaId}", 
                    codigo, empresaId);
                throw;
            }
        }

        public async Task<List<ProductBase>> GetByCodigosAsync(List<string> codigos, int empresaId)
        {
            try
            {
                _logger.LogInformation("Buscando {CodigoCount} productos existentes para empresa {EmpresaId}. Primeros códigos: [{Codigos}]", 
                    codigos.Count, empresaId, string.Join(", ", codigos.Take(5)));

                var productModels = await _context.ProductosBases
                    .AsNoTracking() // Optimización: no tracking ya que solo leemos para comparar
                    .Where(p => codigos.Contains(p.Codigo) && 
                               p.AdministradoPorEmpresaId == empresaId)
                    .ToListAsync();

                _logger.LogInformation("Encontrados {FoundCount} productos existentes de {RequestedCount} solicitados para empresa {EmpresaId}", 
                    productModels.Count, codigos.Count, empresaId);
                
                if (productModels.Count == 0 && codigos.Count > 0)
                {
                    // Debug: verificar si existen productos con esos códigos pero con diferente empresa
                    var anyProducts = await _context.ProductosBases
                        .AsNoTracking()
                        .Where(p => codigos.Take(3).Contains(p.Codigo))
                        .Select(p => new { p.Codigo, p.AdministradoPorEmpresaId })
                        .ToListAsync();
                    
                    _logger.LogWarning("Debug - Productos con códigos {Codigos} encontrados: {Products}", 
                        string.Join(", ", codigos.Take(3)), 
                        string.Join(", ", anyProducts.Select(p => $"Codigo:{p.Codigo}/Empresa:{p.AdministradoPorEmpresaId}")));
                }

                return productModels.Select(MapToDomain).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos por códigos para empresa {EmpresaId}", empresaId);
                throw;
            }
        }

        public async Task<ProductBase> CreateAsync(ProductBase product)
        {
            try
            {
                var productModel = MapToInfrastructure(product);
                
                _context.ProductosBases.Add(productModel);
                await _context.SaveChangesAsync();

                _logger.LogDebug("Producto creado: {Codigo} para empresa {EmpresaId}", 
                    productModel.Codigo, productModel.AdministradoPorEmpresaId);
                
                return MapToDomain(productModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear producto {Codigo}", product.Codigo);
                throw;
            }
        }

        public async Task UpdateAsync(ProductBase product)
        {
            try
            {
                var existingModel = await _context.ProductosBases
                    .FirstOrDefaultAsync(p => p.Id == product.Id);

                if (existingModel == null)
                {
                    throw new InvalidOperationException($"Producto con ID {product.Id} no encontrado para actualizar");
                }

                // Actualizar TODOS los campos (incluye campos de Gecom y campos web)
                existingModel.Descripcion = product.Descripcion;
                existingModel.CodigoRubro = product.CodigoRubro;
                // existingModel.Precio = product.Precio; // ELIMINADO: precio ahora está en productos_base_precios
                // existingModel.Existencia = product.Existencia; // TODO: Mover a ProductoBaseStockRepository
                existingModel.Grupo1 = product.Grupo1;
                existingModel.Grupo2 = product.Grupo2;
                existingModel.Grupo3 = product.Grupo3;
                existingModel.FechaAlta = product.FechaAlta;
                existingModel.FechaModi = product.FechaModi;
                existingModel.Imputable = product.Imputable;
                existingModel.Disponible = product.Disponible;
                existingModel.CodigoUbicacion = product.CodigoUbicacion;
                existingModel.ActualizadoGecom = product.ActualizadoGecom;
                
                // Actualizar campos web
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

                _logger.LogDebug("Producto actualizado: Id={Id}, Codigo={Codigo}", 
                    product.Id, product.Codigo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar producto {Id}", product.Id);
                throw;
            }
        }

        public async Task UpdateFromSyncAsync(ProductBase product)
        {
            try
            {
                var existingModel = await _context.ProductosBases
                    .FirstOrDefaultAsync(p => p.Codigo == product.Codigo && 
                                            p.AdministradoPorEmpresaId == product.AdministradoPorEmpresaId);

                if (existingModel == null)
                {
                    throw new InvalidOperationException($"Producto {product.Codigo} no encontrado para actualizar");
                }

                // Actualizar SOLO campos de Gecom - CRÍTICO para preservar configuraciones web
                existingModel.Descripcion = product.Descripcion;
                existingModel.CodigoRubro = product.CodigoRubro;
                // existingModel.Precio = product.Precio; // ELIMINADO: precio ahora está en productos_base_precios
                // existingModel.Existencia = product.Existencia; // TODO: Mover a ProductoBaseStockRepository
                existingModel.Grupo1 = product.Grupo1;
                existingModel.Grupo2 = product.Grupo2;
                existingModel.Grupo3 = product.Grupo3;
                existingModel.FechaAlta = product.FechaAlta;
                existingModel.FechaModi = product.FechaModi;
                existingModel.Imputable = product.Imputable;
                existingModel.Disponible = product.Disponible;
                existingModel.CodigoUbicacion = product.CodigoUbicacion;
                existingModel.ActualizadoGecom = product.ActualizadoGecom;
                existingModel.UpdatedAt = DateTime.UtcNow;

                // NO actualizar: Visible, Destacado, OrdenCategoria, ImagenUrl, ImagenAlt,
                // DescripcionCorta, DescripcionLarga, Tags, CodigoBarras, Marca, UnidadMedida

                await _context.SaveChangesAsync();

                _logger.LogDebug("Producto actualizado desde sync: {Codigo} para empresa {EmpresaId}", 
                    product.Codigo, product.AdministradoPorEmpresaId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar producto desde sync {Codigo}", product.Codigo);
                throw;
            }
        }

        public async Task<BulkOperationResult> BulkCreateOrUpdateAsync(List<ProductBase> products)
        {
            var result = new BulkOperationResult();
            
            if (!products.Any()) return result;

            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                var empresaId = products.First().AdministradoPorEmpresaId;
                var codigos = products.Select(p => p.Codigo).ToList();

                // Obtener productos existentes en una sola consulta
                var existingProducts = await _context.ProductosBases
                    .Where(p => codigos.Contains(p.Codigo) && 
                               p.AdministradoPorEmpresaId == empresaId)
                    .ToDictionaryAsync(p => p.Codigo, p => p);

                _logger.LogDebug("Encontrados {ExistingCount} productos existentes de {TotalCount} productos en el lote", 
                    existingProducts.Count, products.Count);

                var productosParaCrear = new List<Infrastructure.Models.ProductosBase>();
                
                foreach (var product in products)
                {
                    try
                    {
                        if (existingProducts.TryGetValue(product.Codigo, out var existing))
                        {
                            // Actualizar producto existente - SOLO campos de Gecom
                            existing.Descripcion = product.Descripcion;
                            existing.CodigoRubro = product.CodigoRubro;
                            // existing.Precio = product.Precio; // ELIMINADO: precio ahora está en productos_base_precios
                            // existing.Existencia = product.Existencia; // TODO: Mover a ProductoBaseStockRepository
                            existing.Grupo1 = product.Grupo1;
                            existing.Grupo2 = product.Grupo2;
                            existing.Grupo3 = product.Grupo3;
                            existing.FechaAlta = product.FechaAlta;
                            existing.FechaModi = product.FechaModi;
                            existing.Imputable = product.Imputable;
                            existing.Disponible = product.Disponible;
                            existing.CodigoUbicacion = product.CodigoUbicacion;
                            existing.ActualizadoGecom = product.ActualizadoGecom;
                            existing.UpdatedAt = DateTime.UtcNow;

                            result.Updated++;
                        }
                        else
                        {
                            // Preparar para crear nuevo producto
                            var newProductModel = MapToInfrastructure(product);
                            productosParaCrear.Add(newProductModel);
                            result.Created++;
                        }
                    }
                    catch (Exception ex)
                    {
                        result.Failed++;
                        result.Errors.Add(new BulkOperationError
                        {
                            ProductCode = product.Codigo.ToString(),
                            ErrorMessage = ex.Message,
                            ErrorType = ClassifyError(ex)
                        });

                        _logger.LogError(ex, "Error en bulk operation para producto {Codigo}", product.Codigo);
                    }
                }

                // Usar AddRange para operaciones bulk más eficientes
                if (productosParaCrear.Count > 0)
                {
                    _logger.LogDebug("Agregando {Count} productos nuevos usando AddRange", productosParaCrear.Count);
                    await _context.ProductosBases.AddRangeAsync(productosParaCrear);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Bulk operation completada: {Created} creados, {Updated} actualizados, {Failed} fallidos",
                    result.Created, result.Updated, result.Failed);

                return result;
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error en bulk operation de productos");
                throw;
            }
        }

        public async Task<bool> ExistsByCodigoAsync(string codigo, int empresaId)
        {
            try
            {
                return await _context.ProductosBases
                    .AnyAsync(p => p.Codigo == codigo && 
                                  p.AdministradoPorEmpresaId == empresaId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de producto {Codigo} para empresa {EmpresaId}", 
                    codigo, empresaId);
                throw;
            }
        }

        public async Task<bool> ExistsByCodigoInPrincipalCompanyAsync(string codigo, int empresaPrincipalId)
        {
            try
            {
                return await _context.ProductosBases
                    .AnyAsync(p => p.Codigo == codigo && p.AdministradoPorEmpresaId == empresaPrincipalId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de producto {Codigo} en empresa principal {EmpresaPrincipalId}", codigo, empresaPrincipalId);
                throw;
            }
        }

        public async Task<int> GetCountByEmpresaAsync(int empresaId)
        {
            try
            {
                return await _context.ProductosBases
                    .CountAsync(p => p.AdministradoPorEmpresaId == empresaId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al contar productos para empresa {EmpresaId}", empresaId);
                throw;
            }
        }

        public async Task<(List<ProductBase> products, int total)> GetPagedAsync(
            bool? visible,
            bool? destacado,
            int? codigoRubro,
            string? busqueda,
            int page,
            int pageSize,
            string? sortBy = null,
            string? sortOrder = null,
            int? listaPrecioIdForSorting = null,
            bool incluirSinExistencia = false,
            bool? soloSinConfiguracion = null)
        {
            try
            {
                var query = _context.ProductosBases.AsQueryable();

                if (visible.HasValue)
                    query = query.Where(p => (p.Visible ?? false) == visible.Value);

                if (destacado.HasValue)
                    query = query.Where(p => (p.Destacado ?? false) == destacado.Value);

                if (codigoRubro.HasValue)
                    query = query.Where(p => p.CodigoRubro == codigoRubro.Value);

                if (!string.IsNullOrEmpty(busqueda))
                {
                    // Buscar por código exacto o en descripción
                    query = query.Where(p => p.Codigo == busqueda ||
                                           p.Descripcion.Contains(busqueda) || 
                                           (p.DescripcionCorta != null && p.DescripcionCorta.Contains(busqueda)) ||
                                           (p.Tags != null && p.Tags.Contains(busqueda)));
                }

                // Filtro de existencia (por defecto solo con existencia > 0)
                // if (!incluirSinExistencia)
                //     query = query.Where(p => p.Existencia > 0); // TODO: Usar ProductoBaseStockRepository para filtrar

                // Filtro de solo productos sin configuración
                if (soloSinConfiguracion == true)
                    query = query.Where(p => string.IsNullOrEmpty(p.ImagenUrl));

                var total = await query.CountAsync();
                
                // Aplicar ordenamiento
                if (!string.IsNullOrEmpty(sortBy))
                {
                    var isDescending = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);
                    
                    // Si se ordena por precio, necesitamos hacer JOIN con tabla de precios
                    if (sortBy.ToLower() == "precio" && listaPrecioIdForSorting.HasValue)
                    {
                        var queryWithPrices = from p in query
                                            join precio in _context.ProductosBasePrecios
                                                on new { ProductoId = p.Id, ListaId = listaPrecioIdForSorting.Value }
                                                equals new { ProductoId = precio.ProductoBaseId, ListaId = precio.ListaPrecioId }
                                                into precios
                                            from precio in precios.DefaultIfEmpty()
                                            select new { Product = p, Precio = precio != null ? precio.Precio : 0 };

                        var orderedWithPrices = isDescending ? 
                            queryWithPrices.OrderByDescending(x => x.Precio) : 
                            queryWithPrices.OrderBy(x => x.Precio);

                        var productsWithPrices = await orderedWithPrices
                            .Skip((page - 1) * pageSize)
                            .Take(pageSize)
                            .Select(x => x.Product)
                            .Include(p => p.AdministradoPorEmpresa)
                            .Include(p => p.CodigoRubroNavigation)
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
                                query.OrderByDescending(p => p.Id) : // TODO: Ordenar por stock desde ProductoBaseStockRepository
                                query.OrderBy(p => p.Id), // TODO: Ordenar por stock desde ProductoBaseStockRepository
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
                    .Include(p => p.AdministradoPorEmpresa)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();
                    
                var domainProducts = products.Select(MapToDomain).ToList();

                return (domainProducts, total);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener productos paginados");
                throw;
            }
        }

        public async Task DeleteAsync(ProductBase product)
        {
            try
            {
                var productModel = await _context.ProductosBases
                    .FirstOrDefaultAsync(p => p.Id == product.Id);

                if (productModel != null)
                {
                    _context.ProductosBases.Remove(productModel);
                    await _context.SaveChangesAsync();
                    
                    _logger.LogDebug("Producto eliminado: Id={Id}, Codigo={Codigo}", 
                        productModel.Id, productModel.Codigo);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar producto {Id}", product.Id);
                throw;
            }
        }

        private string ClassifyError(Exception ex)
        {
            var message = ex.Message.ToLower();
            
            return message switch
            {
                _ when message.Contains("unique") || message.Contains("duplicate") => "duplicate_error",
                _ when message.Contains("foreign") || message.Contains("constraint") => "constraint_error",
                _ when message.Contains("validation") => "validation_error",
                _ when message.Contains("timeout") => "timeout_error",
                _ => "unknown_error"
            };
        }

        private ProductBase MapToDomain(Infrastructure.Models.ProductosBase model)
        {
            // Usar reflection para crear instancia y establecer propiedades privadas usando backing fields
            var product = Activator.CreateInstance(typeof(ProductBase), true) as ProductBase;
            var productType = typeof(ProductBase);

            _logger.LogDebug("Mapeando ProductBase desde DB: Id={Id}, Codigo={Codigo}", model.Id, model.Codigo);

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
                        _logger.LogDebug("No se pudo establecer propiedad {PropertyName} en ProductBase", propertyName);
                    }
                }
            }

            SetProperty("Id", model.Id);
            SetProperty("Codigo", model.Codigo);
            SetProperty("Descripcion", model.Descripcion ?? "");
            SetProperty("CodigoRubro", model.CodigoRubro ?? 0);
            // SetProperty("Precio", model.Precio ?? 0); // ELIMINADO: precio ahora está en productos_base_precios
            // SetProperty("Existencia", model.Existencia ?? 0); // TODO: Manejar en ProductoBaseStockRepository
            SetProperty("Grupo1", model.Grupo1);
            SetProperty("Grupo2", model.Grupo2);
            SetProperty("Grupo3", model.Grupo3);
            SetProperty("FechaAlta", model.FechaAlta ?? DateTime.UtcNow);
            SetProperty("FechaModi", model.FechaModi ?? DateTime.UtcNow);
            SetProperty("Imputable", model.Imputable ?? "S");
            SetProperty("Disponible", model.Disponible ?? "S");
            SetProperty("CodigoUbicacion", model.CodigoUbicacion ?? string.Empty);

            // Campos web
            SetProperty("Visible", model.Visible ?? true);
            SetProperty("Destacado", model.Destacado ?? false);
            SetProperty("OrdenCategoria", model.OrdenCategoria ?? 0);
            SetProperty("ImagenUrl", model.ImagenUrl ?? string.Empty);
            SetProperty("ImagenAlt", model.ImagenAlt ?? string.Empty);
            SetProperty("DescripcionCorta", model.DescripcionCorta ?? string.Empty);
            SetProperty("DescripcionLarga", model.DescripcionLarga ?? string.Empty);
            SetProperty("Tags", model.Tags ?? string.Empty);
            SetProperty("CodigoBarras", model.CodigoBarras ?? string.Empty);
            SetProperty("Marca", model.Marca ?? string.Empty);
            SetProperty("UnidadMedida", model.UnidadMedida ?? "UN");

            // Control
            SetProperty("AdministradoPorEmpresaId", model.AdministradoPorEmpresaId);
            SetProperty("ActualizadoGecom", model.ActualizadoGecom ?? DateTime.UtcNow);

            // Base
            SetProperty("CreatedAt", model.CreatedAt ?? DateTime.UtcNow);
            SetProperty("UpdatedAt", model.UpdatedAt ?? DateTime.UtcNow);

            _logger.LogDebug("ProductBase mapeado exitosamente: Id={Id}, Codigo={Codigo}", product?.Id, product?.Codigo);

            return product ?? throw new InvalidOperationException("Error al mapear ProductBase desde DB");
        }

        private Infrastructure.Models.ProductosBase MapToInfrastructure(ProductBase domain)
        {
            return new Infrastructure.Models.ProductosBase
            {
                Codigo = domain.Codigo,
                Descripcion = domain.Descripcion,
                CodigoRubro = domain.CodigoRubro,
                // Precio = domain.Precio, // ELIMINADO: precio ahora está en productos_base_precios
                // Existencia = domain.Existencia, // TODO: Obtener desde ProductoBaseStockRepository
                Grupo1 = domain.Grupo1,
                Grupo2 = domain.Grupo2,
                Grupo3 = domain.Grupo3,
                FechaAlta = domain.FechaAlta,
                FechaModi = domain.FechaModi,
                Imputable = domain.Imputable,
                Disponible = domain.Disponible,
                CodigoUbicacion = domain.CodigoUbicacion,
                
                // Campos web
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
                
                // Control
                AdministradoPorEmpresaId = domain.AdministradoPorEmpresaId,
                ActualizadoGecom = domain.ActualizadoGecom,
                
                // Timestamps
                CreatedAt = domain.CreatedAt,
                UpdatedAt = domain.UpdatedAt
            };
        }
    }
}