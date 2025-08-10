using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using DistriCatalogoAPI.Domain.Entities;
using DistriCatalogoAPI.Infrastructure.Models;
using DistriCatalogoAPI.Domain.Interfaces;

namespace DistriCatalogoAPI.Infrastructure.Repositories
{
    public class CategoryEmpresaRepository : ICategoryEmpresaRepository
    {
        private readonly DistricatalogoContext _context;
        private readonly ILogger<CategoryEmpresaRepository> _logger;

        public CategoryEmpresaRepository(
            DistricatalogoContext context,
            ILogger<CategoryEmpresaRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<IReadOnlyList<CategoryEmpresa>> GetAllAsync()
        {
            try
            {
                var categoryModels = await _context.CategoriasEmpresas
                    .OrderBy(c => c.EmpresaId)
                    .ThenBy(c => c.Orden)
                    .ThenBy(c => c.Nombre)
                    .ToListAsync();

                return categoryModels.Select(MapToDomain).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener todas las categorías empresa");
                throw;
            }
        }

        public async Task<IReadOnlyList<CategoryEmpresa>> GetByEmpresaIdAsync(int empresaId)
        {
            try
            {
                var categoryModels = await _context.CategoriasEmpresas
                    .Where(c => c.EmpresaId == empresaId)
                    .OrderBy(c => c.Orden)
                    .ThenBy(c => c.Nombre)
                    .ToListAsync();

                return categoryModels.Select(MapToDomain).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener categorías empresa por EmpresaId {EmpresaId}", empresaId);
                throw;
            }
        }

        public async Task<CategoryEmpresa?> GetByIdAsync(int id)
        {
            try
            {
                var categoryModel = await _context.CategoriasEmpresas
                    .FirstOrDefaultAsync(c => c.Id == id);

                return categoryModel != null ? MapToDomain(categoryModel) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener categoría empresa por Id {Id}", id);
                throw;
            }
        }

        public async Task<CategoryEmpresa?> GetByEmpresaAndCodigoRubroAsync(int empresaId, int codigoRubro)
        {
            try
            {
                var categoryModel = await _context.CategoriasEmpresas
                    .FirstOrDefaultAsync(c => c.EmpresaId == empresaId && c.CodigoRubro == codigoRubro);

                return categoryModel != null ? MapToDomain(categoryModel) : null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener categoría empresa por EmpresaId {EmpresaId} y CodigoRubro {CodigoRubro}", empresaId, codigoRubro);
                throw;
            }
        }

        public async Task<IReadOnlyList<CategoryEmpresa>> GetVisibleByEmpresaIdAsync(int empresaId)
        {
            try
            {
                var categoryModels = await _context.CategoriasEmpresas
                    .Where(c => c.EmpresaId == empresaId && c.Visible == true)
                    .OrderBy(c => c.Orden)
                    .ThenBy(c => c.Nombre)
                    .ToListAsync();

                return categoryModels.Select(MapToDomain).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener categorías empresa visibles por EmpresaId {EmpresaId}", empresaId);
                throw;
            }
        }

        public async Task<bool> ExistsByEmpresaAndCodigoRubroAsync(int empresaId, int codigoRubro)
        {
            try
            {
                return await _context.CategoriasEmpresas
                    .AnyAsync(c => c.EmpresaId == empresaId && c.CodigoRubro == codigoRubro);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de categoría empresa EmpresaId {EmpresaId} CodigoRubro {CodigoRubro}", empresaId, codigoRubro);
                throw;
            }
        }

        public async Task<bool> ExistsByCodigoRubroInPrincipalCompanyAsync(int codigoRubro, int empresaPrincipalId)
        {
            try
            {
                return await _context.CategoriasEmpresas
                    .Join(_context.Empresas, ce => ce.EmpresaId, e => e.Id, (ce, e) => new { ce, e })
                    .AnyAsync(joined => joined.ce.CodigoRubro == codigoRubro && 
                                      (joined.e.Id == empresaPrincipalId || joined.e.EmpresaPrincipalId == empresaPrincipalId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de categoría empresa código rubro {CodigoRubro} en empresa principal {EmpresaPrincipalId}", codigoRubro, empresaPrincipalId);
                throw;
            }
        }

        public async Task<bool> CategoryExistsForCompanyAsync(int codigoRubro, int empresaId, int empresaPrincipalId)
        {
            try
            {
                // Para productos empresa: Buscar en categorías base de la empresa principal
                var existsInBase = await _context.CategoriasBases
                    .AnyAsync(c => c.CodigoRubro == codigoRubro && c.CreatedByEmpresaId == empresaPrincipalId);

                if (existsInBase)
                    return true;

                // Buscar en categorías empresa de la empresa específica (principal o cliente)
                var existsInEmpresa = await _context.CategoriasEmpresas
                    .AnyAsync(ce => ce.CodigoRubro == codigoRubro && ce.EmpresaId == empresaId);

                return existsInEmpresa;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al verificar existencia de categoría {CodigoRubro} para empresa {EmpresaId} en empresa principal {EmpresaPrincipalId}", codigoRubro, empresaId, empresaPrincipalId);
                throw;
            }
        }

        public async Task<CategoryEmpresa> AddAsync(CategoryEmpresa category)
        {
            try
            {
                var categoryModel = MapToInfrastructure(category);
                
                _context.CategoriasEmpresas.Add(categoryModel);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Categoría empresa creada: EmpresaId {EmpresaId}, CodigoRubro {CodigoRubro}, Nombre '{Nombre}'", 
                    categoryModel.EmpresaId, categoryModel.CodigoRubro, categoryModel.Nombre);
                
                return MapToDomain(categoryModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al crear categoría empresa EmpresaId {EmpresaId} CodigoRubro {CodigoRubro}", 
                    category.EmpresaId, category.CodigoRubro);
                throw;
            }
        }

        public async Task<CategoryEmpresa> UpdateAsync(CategoryEmpresa category)
        {
            try
            {
                var existingModel = await _context.CategoriasEmpresas.FindAsync(category.Id);
                if (existingModel == null)
                    throw new InvalidOperationException($"Categoría empresa con ID {category.Id} no encontrada");

                // Actualizar propiedades
                existingModel.CodigoRubro = category.CodigoRubro;
                existingModel.Nombre = category.Nombre;
                existingModel.Icono = category.Icono;
                existingModel.Visible = category.Visible;
                existingModel.Orden = category.Orden;
                existingModel.Color = category.Color;
                existingModel.Descripcion = category.Descripcion;
                existingModel.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Categoría empresa actualizada: ID {Id}, EmpresaId {EmpresaId}, CodigoRubro {CodigoRubro}", 
                    category.Id, category.EmpresaId, category.CodigoRubro);
                
                return MapToDomain(existingModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al actualizar categoría empresa ID {Id}", category.Id);
                throw;
            }
        }

        public async Task DeleteAsync(int id)
        {
            try
            {
                var category = await _context.CategoriasEmpresas.FindAsync(id);
                if (category != null)
                {
                    _context.CategoriasEmpresas.Remove(category);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Categoría empresa eliminada: ID {Id}", id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al eliminar categoría empresa ID {Id}", id);
                throw;
            }
        }

        public async Task<int> CountProductsAsync(int categoryId)
        {
            try
            {
                // Buscar la categoría para obtener el código de rubro
                var category = await _context.CategoriasEmpresas.FindAsync(categoryId);
                if (category == null) return 0;

                // Contar productos de la empresa que usen este código de rubro
                return await _context.ProductosEmpresas
                    .CountAsync(p => p.EmpresaId == category.EmpresaId && p.CodigoRubro == category.CodigoRubro);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al contar productos para categoría empresa {CategoryId}", categoryId);
                throw;
            }
        }

        private CategoryEmpresa MapToDomain(Infrastructure.Models.CategoriasEmpresa model)
        {
            // Usar reflection para crear instancia y establecer propiedades privadas
            var category = Activator.CreateInstance(typeof(CategoryEmpresa), true) as CategoryEmpresa;
            var categoryType = typeof(CategoryEmpresa);

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
            SetProperty("EmpresaId", model.EmpresaId);
            SetProperty("CodigoRubro", model.CodigoRubro);
            SetProperty("Nombre", model.Nombre ?? "");
            SetProperty("Icono", model.Icono);
            SetProperty("Visible", model.Visible ?? true);
            SetProperty("Orden", model.Orden ?? 0);
            SetProperty("Color", model.Color ?? "#6B7280");
            SetProperty("Descripcion", model.Descripcion);
            SetProperty("CreatedAt", model.CreatedAt ?? DateTime.UtcNow);
            SetProperty("UpdatedAt", model.UpdatedAt ?? DateTime.UtcNow);

            return category;
        }

        private Infrastructure.Models.CategoriasEmpresa MapToInfrastructure(CategoryEmpresa domain)
        {
            return new Infrastructure.Models.CategoriasEmpresa
            {
                EmpresaId = domain.EmpresaId,
                CodigoRubro = domain.CodigoRubro,
                Nombre = domain.Nombre,
                Icono = domain.Icono,
                Visible = domain.Visible,
                Orden = domain.Orden,
                Color = domain.Color,
                Descripcion = domain.Descripcion,
                CreatedAt = domain.CreatedAt,
                UpdatedAt = domain.UpdatedAt
            };
        }
    }
}