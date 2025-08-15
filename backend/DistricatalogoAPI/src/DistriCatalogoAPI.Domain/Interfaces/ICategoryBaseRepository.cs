using System.Collections.Generic;
using System.Threading.Tasks;
using DistriCatalogoAPI.Domain.Entities;

namespace DistriCatalogoAPI.Domain.Interfaces
{
    public interface ICategoryBaseRepository
    {
        Task<CategoryBase?> GetByIdAsync(int id);
        Task<CategoryBase?> GetByCodigoRubroAsync(int codigoRubro);
        Task<List<CategoryBase>> GetByCodigosRubroAsync(List<int> codigosRubro);
        Task<List<CategoryBase>> GetByEmpresaAsync(int empresaId);
        Task<CategoryBase> CreateAsync(CategoryBase category);
        Task<CategoryBase> UpdateAsync(CategoryBase category);
        Task<List<CategoryBase>> BulkCreateAsync(List<CategoryBase> categories);
        Task UpdateBulkAsync(List<CategoryBase> categories);
        Task<bool> ExistsByCodigoRubroAsync(int codigoRubro);
        Task<bool> ExistsByCodigoRubroInPrincipalCompanyAsync(int codigoRubro, int empresaPrincipalId);
        Task<bool> CategoryExistsInPrincipalCompanyAsync(int codigoRubro, int empresaPrincipalId);
        Task<Dictionary<int, bool>> CheckExistenceAsync(List<int> codigosRubro);
        Task<int> GetCountByEmpresaAsync(int empresaId);
        Task<int> GetProductCountByCategoryAsync(int codigoRubro);
    }
}