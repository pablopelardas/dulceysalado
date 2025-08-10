using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DistriCatalogoAPI.Domain.Entities;
using DistriCatalogoAPI.Domain.Interfaces;
using DistriCatalogoAPI.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DistriCatalogoAPI.Infrastructure.Repositories
{
    public class FeatureRepository : IFeatureRepository
    {
        private readonly DistricatalogoContext _context;
        private readonly ILogger<FeatureRepository> _logger;

        public FeatureRepository(DistricatalogoContext context, ILogger<FeatureRepository> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Feature Definitions
        public async Task<IEnumerable<FeatureDefinition>> GetAllDefinitionsAsync()
        {
            return await _context.FeatureDefinitions
                .Where(fd => fd.Activo)
                .OrderBy(fd => fd.Categoria)
                .ThenBy(fd => fd.Nombre)
                .ToListAsync();
        }

        public async Task<FeatureDefinition?> GetDefinitionByCodigoAsync(string codigo)
        {
            return await _context.FeatureDefinitions
                .FirstOrDefaultAsync(fd => fd.Codigo == codigo && fd.Activo);
        }

        public async Task<FeatureDefinition?> GetDefinitionByIdAsync(int id)
        {
            return await _context.FeatureDefinitions
                .FirstOrDefaultAsync(fd => fd.Id == id);
        }

        public async Task<FeatureDefinition> CreateDefinitionAsync(FeatureDefinition definition)
        {
            definition.CreatedAt = DateTime.UtcNow;
            definition.UpdatedAt = DateTime.UtcNow;
            
            _context.FeatureDefinitions.Add(definition);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Feature definition created: {Codigo}", definition.Codigo);
            return definition;
        }

        public async Task<FeatureDefinition> UpdateDefinitionAsync(FeatureDefinition definition)
        {
            definition.UpdatedAt = DateTime.UtcNow;
            
            _context.FeatureDefinitions.Update(definition);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Feature definition updated: {Codigo}", definition.Codigo);
            return definition;
        }

        // Empresa Features
        public async Task<IEnumerable<EmpresaFeature>> GetByEmpresaIdAsync(int empresaId)
        {
            return await _context.EmpresaFeatures
                .Include(ef => ef.Feature)
                .Where(ef => ef.EmpresaId == empresaId)
                .OrderBy(ef => ef.Feature.Categoria)
                .ThenBy(ef => ef.Feature.Nombre)
                .ToListAsync();
        }

        public async Task<EmpresaFeature?> GetByEmpresaAndCodigoAsync(int empresaId, string codigo)
        {
            return await _context.EmpresaFeatures
                .Include(ef => ef.Feature)
                .FirstOrDefaultAsync(ef => ef.EmpresaId == empresaId && ef.Feature.Codigo == codigo);
        }

        public async Task<EmpresaFeature?> GetByEmpresaAndFeatureIdAsync(int empresaId, int featureId)
        {
            return await _context.EmpresaFeatures
                .Include(ef => ef.Feature)
                .FirstOrDefaultAsync(ef => ef.EmpresaId == empresaId && ef.FeatureId == featureId);
        }

        public async Task<EmpresaFeature> ConfigureFeatureAsync(EmpresaFeature feature)
        {
            feature.CreatedAt = DateTime.UtcNow;
            feature.UpdatedAt = DateTime.UtcNow;
            
            _context.EmpresaFeatures.Add(feature);
            await _context.SaveChangesAsync();
            
            // Load the feature definition for logging
            await _context.Entry(feature)
                .Reference(ef => ef.Feature)
                .LoadAsync();
                
            _logger.LogInformation("Feature configured for empresa {EmpresaId}: {Codigo} = {Habilitado}", 
                feature.EmpresaId, feature.Feature.Codigo, feature.Habilitado);
                
            return feature;
        }

        public async Task<EmpresaFeature> UpdateFeatureAsync(EmpresaFeature feature)
        {
            feature.UpdatedAt = DateTime.UtcNow;
            
            _context.EmpresaFeatures.Update(feature);
            await _context.SaveChangesAsync();
            
            // Load the feature definition for logging
            if (feature.Feature == null)
            {
                await _context.Entry(feature)
                    .Reference(ef => ef.Feature)
                    .LoadAsync();
            }
                
            _logger.LogInformation("Feature updated for empresa {EmpresaId}: {Codigo} = {Habilitado}", 
                feature.EmpresaId, feature.Feature.Codigo, feature.Habilitado);
                
            return feature;
        }

        public async Task<bool> DeleteFeatureAsync(int empresaId, string codigo)
        {
            var feature = await GetByEmpresaAndCodigoAsync(empresaId, codigo);
            if (feature == null)
                return false;
                
            _context.EmpresaFeatures.Remove(feature);
            await _context.SaveChangesAsync();
            
            _logger.LogInformation("Feature deleted for empresa {EmpresaId}: {Codigo}", 
                empresaId, codigo);
                
            return true;
        }

        // Bulk operations
        public async Task<Dictionary<string, EmpresaFeature>> GetFeaturesDictionaryAsync(int empresaId)
        {
            var features = await _context.EmpresaFeatures
                .Include(ef => ef.Feature)
                .Where(ef => ef.EmpresaId == empresaId)
                .ToListAsync();
                
            return features.ToDictionary(ef => ef.Feature.Codigo, ef => ef);
        }

        public async Task<IEnumerable<EmpresaFeature>> GetByEmpresaIdsAsync(IEnumerable<int> empresaIds)
        {
            return await _context.EmpresaFeatures
                .Include(ef => ef.Feature)
                .Where(ef => empresaIds.Contains(ef.EmpresaId))
                .ToListAsync();
        }

        // Utility
        public async Task<bool> FeatureExistsAsync(string codigo)
        {
            return await _context.FeatureDefinitions
                .AnyAsync(fd => fd.Codigo == codigo && fd.Activo);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        // Get features with defaults for a company
        public async Task<List<EmpresaFeature>> GetFeaturesWithDefaultsAsync(int empresaId)
        {
            // Obtener todas las definiciones de features activas
            var definitions = await GetAllDefinitionsAsync();
            
            // Obtener configuraciones específicas de la empresa
            var empresaFeatures = await GetFeaturesDictionaryAsync(empresaId);
            
            var result = new List<EmpresaFeature>();
            
            foreach (var definition in definitions)
            {
                if (empresaFeatures.TryGetValue(definition.Codigo, out var empresaFeature))
                {
                    // Ya existe configuración para esta empresa
                    result.Add(empresaFeature);
                }
                else
                {
                    // Crear feature con valores por defecto
                    var defaultFeature = new EmpresaFeature
                    {
                        EmpresaId = empresaId,
                        FeatureId = definition.Id,
                        Feature = definition,
                        Habilitado = false,
                        Valor = definition.ValorDefecto,
                        Metadata = null,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow
                    };
                    result.Add(defaultFeature);
                }
            }
            
            return result;
        }
    }
}