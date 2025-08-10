using DistriCatalogoAPI.Domain.Entities;

namespace DistriCatalogoAPI.Domain.Interfaces
{
    public interface ICategoryEmpresaRepository
    {
        Task<IReadOnlyList<CategoryEmpresa>> GetAllAsync();
        Task<IReadOnlyList<CategoryEmpresa>> GetByEmpresaIdAsync(int empresaId);
        Task<CategoryEmpresa?> GetByIdAsync(int id);
        Task<CategoryEmpresa?> GetByEmpresaAndCodigoRubroAsync(int empresaId, int codigoRubro);
        Task<IReadOnlyList<CategoryEmpresa>> GetVisibleByEmpresaIdAsync(int empresaId);
        Task<bool> ExistsByEmpresaAndCodigoRubroAsync(int empresaId, int codigoRubro);
        Task<bool> ExistsByCodigoRubroInPrincipalCompanyAsync(int codigoRubro, int empresaPrincipalId);
        Task<bool> CategoryExistsForCompanyAsync(int codigoRubro, int empresaId, int empresaPrincipalId);
        Task<CategoryEmpresa> AddAsync(CategoryEmpresa category);
        Task<CategoryEmpresa> UpdateAsync(CategoryEmpresa category);
        Task DeleteAsync(int id);
        Task<int> CountProductsAsync(int categoryId);
    }
}