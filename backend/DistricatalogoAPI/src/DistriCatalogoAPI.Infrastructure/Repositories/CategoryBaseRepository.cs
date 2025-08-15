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
    public class CategoryBaseRepository : ICategoryBaseRepository
    {
        private readonly DistricatalogoContext _context;
        private readonly ILogger<CategoryBaseRepository> _logger;

        public CategoryBaseRepository(
            DistricatalogoContext context,
            ILogger<CategoryBaseRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<CategoryBase?> GetByIdAsync(int id)
        {
            try
            {
                var categoryModel = await _context.CategoriasBases
                    .FirstOrDefaultAsync(c => c.Id == id);

                return categoryModel != null ? MapToDomain(categoryModel) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener categoría base por ID {Id}", id);
                throw;
            }
        }

        public async Task<CategoryBase?> GetByCodigoRubroAsync(int codigoRubro)
        {
            try
            {
                var categoryModel = await _context.CategoriasBases
                    .FirstOrDefaultAsync(c => c.CodigoRubro == codigoRubro);

                return categoryModel != null ? MapToDomain(categoryModel) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener categoría por código rubro {CodigoRubro}", codigoRubro);
                throw;
            }
        }

        public async Task<List<CategoryBase>> GetByCodigosRubroAsync(List<int> codigosRubro)
        {
            try
            {
                var categoryModels = await _context.CategoriasBases
                    .Where(c => codigosRubro.Contains(c.CodigoRubro))
                    .ToListAsync();

                return categoryModels.Select(MapToDomain).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener categorías por códigos de rubro");
                throw;
            }
        }

        public async Task<List<CategoryBase>> GetByEmpresaAsync(int empresaId)
        {
            try
            {
                var categoryModels = await _context.CategoriasBases
                    .Where(c => c.CreatedByEmpresaId == empresaId)
                    .OrderBy(c => c.Orden)
                    .ThenBy(c => c.Nombre)
                    .ToListAsync();

                return categoryModels.Select(MapToDomain).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener categorías por empresa {EmpresaId}", empresaId);
                throw;
            }
        }

        public async Task<CategoryBase> CreateAsync(CategoryBase category)
        {
            try
            {
                var categoryModel = MapToInfrastructure(category);
                
                _context.CategoriasBases.Add(categoryModel);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Categoría creada: código rubro {CodigoRubro}, nombre '{Nombre}' por empresa {EmpresaId}", 
                    categoryModel.CodigoRubro, categoryModel.Nombre, categoryModel.CreatedByEmpresaId);
                
                return MapToDomain(categoryModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear categoría código rubro {CodigoRubro}", category.CodigoRubro);
                throw;
            }
        }

        public async Task<CategoryBase> UpdateAsync(CategoryBase category)
        {
            try
            {
                var existingCategory = await _context.CategoriasBases
                    .FirstOrDefaultAsync(c => c.Id == category.Id);

                if (existingCategory == null)
                {
                    throw new InvalidOperationException($"Categoría base con ID {category.Id} no encontrada");
                }

                // Actualizar campos
                existingCategory.Nombre = category.Nombre;
                existingCategory.Icono = category.Icono;
                existingCategory.Visible = category.Visible;
                existingCategory.Orden = category.Orden;
                existingCategory.Color = category.Color;
                existingCategory.Descripcion = category.Descripcion;
                existingCategory.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Categoría base actualizada: ID {Id}, código rubro {CodigoRubro}, nombre '{Nombre}'", 
                    category.Id, existingCategory.CodigoRubro, existingCategory.Nombre);
                
                return MapToDomain(existingCategory);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar categoría base ID {Id}", category.Id);
                throw;
            }
        }

        public async Task<List<CategoryBase>> BulkCreateAsync(List<CategoryBase> categories)
        {
            if (!categories.Any()) return new List<CategoryBase>();

            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                var categoryModels = categories.Select(MapToInfrastructure).ToList();
                
                _context.CategoriasBases.AddRange(categoryModels);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Bulk create de categorías completado: {Count} categorías creadas", 
                    categoryModels.Count);

                return categoryModels.Select(MapToDomain).ToList();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error en bulk create de categorías");
                throw;
            }
        }

        public async Task UpdateBulkAsync(List<CategoryBase> categories)
        {
            if (!categories.Any()) return;

            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                foreach (var category in categories)
                {
                    var existingCategory = await _context.CategoriasBases
                        .FirstOrDefaultAsync(c => c.Id == category.Id);

                    if (existingCategory != null)
                    {
                        existingCategory.Nombre = category.Nombre;
                        existingCategory.UpdatedAt = category.UpdatedAt;
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                _logger.LogInformation("Bulk update de categorías completado: {Count} categorías actualizadas", 
                    categories.Count);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error en bulk update de categorías");
                throw;
            }
        }

        public async Task<bool> ExistsByCodigoRubroAsync(int codigoRubro)
        {
            try
            {
                return await _context.CategoriasBases
                    .AnyAsync(c => c.CodigoRubro == codigoRubro);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de categoría código rubro {CodigoRubro}", codigoRubro);
                throw;
            }
        }

        public async Task<bool> ExistsByCodigoRubroInPrincipalCompanyAsync(int codigoRubro, int empresaPrincipalId)
        {
            try
            {
                return await _context.CategoriasBases
                    .AnyAsync(c => c.CodigoRubro == codigoRubro && c.CreatedByEmpresaId == empresaPrincipalId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de categoría base código rubro {CodigoRubro} en empresa principal {EmpresaPrincipalId}", codigoRubro, empresaPrincipalId);
                throw;
            }
        }

        public async Task<bool> CategoryExistsInPrincipalCompanyAsync(int codigoRubro, int empresaPrincipalId)
        {
            try
            {
                // Para productos base: SOLO buscar en categorías base de la empresa principal
                return await _context.CategoriasBases
                    .AnyAsync(c => c.CodigoRubro == codigoRubro && c.CreatedByEmpresaId == empresaPrincipalId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de categoría base {CodigoRubro} en empresa principal {EmpresaPrincipalId}", codigoRubro, empresaPrincipalId);
                throw;
            }
        }

        public async Task<Dictionary<int, bool>> CheckExistenceAsync(List<int> codigosRubro)
        {
            try
            {
                var existingCodes = await _context.CategoriasBases
                    .Where(c => codigosRubro.Contains(c.CodigoRubro))
                    .Select(c => c.CodigoRubro)
                    .ToListAsync();

                return codigosRubro.ToDictionary(
                    codigo => codigo,
                    codigo => existingCodes.Contains(codigo));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de múltiples categorías");
                throw;
            }
        }

        public async Task<int> GetCountByEmpresaAsync(int empresaId)
        {
            try
            {
                return await _context.CategoriasBases
                    .CountAsync(c => c.CreatedByEmpresaId == empresaId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al contar categorías para empresa {EmpresaId}", empresaId);
                throw;
            }
        }

        public async Task<int> GetProductCountByCategoryAsync(int codigoRubro)
        {
            try
            {
                return await _context.ProductosBases
                    .CountAsync(p => p.CodigoRubro == codigoRubro);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al contar productos para categoría código rubro {CodigoRubro}", codigoRubro);
                throw;
            }
        }

        private CategoryBase MapToDomain(Infrastructure.Models.CategoriasBase model)
        {
            // Usar reflection para crear instancia y establecer propiedades privadas
            var category = Activator.CreateInstance(typeof(CategoryBase), true) as CategoryBase;
            var categoryType = typeof(CategoryBase);

            void SetProperty(string propertyName, object value)
            {
                var prop = categoryType.GetProperty(propertyName);
                if (prop != null && prop.CanWrite)
                {
                    prop.SetValue(category, value);
                }
                else
                {
                    var field = categoryType.GetField($"<{propertyName}>k__BackingField", 
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    field?.SetValue(category, value);
                }
            }

            SetProperty("Id", model.Id);
            SetProperty("CodigoRubro", model.CodigoRubro);
            SetProperty("Nombre", model.Nombre ?? "");
            SetProperty("Icono", model.Icono);
            SetProperty("Visible", model.Visible ?? true);
            SetProperty("Orden", model.Orden ?? 0);
            SetProperty("Color", model.Color ?? "#6B7280");
            SetProperty("Descripcion", model.Descripcion);
            SetProperty("CreatedByEmpresaId", model.CreatedByEmpresaId);
            SetProperty("CreatedAt", model.CreatedAt ?? DateTime.UtcNow);
            SetProperty("UpdatedAt", model.UpdatedAt ?? DateTime.UtcNow);

            return category;
        }

        private Infrastructure.Models.CategoriasBase MapToInfrastructure(CategoryBase domain)
        {
            return new Infrastructure.Models.CategoriasBase
            {
                CodigoRubro = domain.CodigoRubro,
                Nombre = domain.Nombre,
                Icono = domain.Icono,
                Visible = domain.Visible,
                Orden = domain.Orden,
                Color = domain.Color,
                Descripcion = domain.Descripcion,
                CreatedByEmpresaId = domain.CreatedByEmpresaId,
                CreatedAt = domain.CreatedAt,
                UpdatedAt = domain.UpdatedAt
            };
        }
    }
}