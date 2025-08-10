using System.Collections.Generic;
using System.Threading.Tasks;
using DistriCatalogoAPI.Domain.Entities;

namespace DistriCatalogoAPI.Domain.Interfaces
{
    public interface IFeatureRepository
    {
        // Feature Definitions
        Task<IEnumerable<FeatureDefinition>> GetAllDefinitionsAsync();
        
        // Get features with defaults for a company
        Task<List<EmpresaFeature>> GetFeaturesWithDefaultsAsync(int empresaId);
        Task<FeatureDefinition?> GetDefinitionByCodigoAsync(string codigo);
        Task<FeatureDefinition?> GetDefinitionByIdAsync(int id);
        Task<FeatureDefinition> CreateDefinitionAsync(FeatureDefinition definition);
        Task<FeatureDefinition> UpdateDefinitionAsync(FeatureDefinition definition);
        
        // Empresa Features
        Task<IEnumerable<EmpresaFeature>> GetByEmpresaIdAsync(int empresaId);
        Task<EmpresaFeature?> GetByEmpresaAndCodigoAsync(int empresaId, string codigo);
        Task<EmpresaFeature?> GetByEmpresaAndFeatureIdAsync(int empresaId, int featureId);
        Task<EmpresaFeature> ConfigureFeatureAsync(EmpresaFeature feature);
        Task<EmpresaFeature> UpdateFeatureAsync(EmpresaFeature feature);
        Task<bool> DeleteFeatureAsync(int empresaId, string codigo);
        
        // Bulk operations
        Task<Dictionary<string, EmpresaFeature>> GetFeaturesDictionaryAsync(int empresaId);
        Task<IEnumerable<EmpresaFeature>> GetByEmpresaIdsAsync(IEnumerable<int> empresaIds);
        
        // Utility
        Task<bool> FeatureExistsAsync(string codigo);
        Task<int> SaveChangesAsync();
    }
}