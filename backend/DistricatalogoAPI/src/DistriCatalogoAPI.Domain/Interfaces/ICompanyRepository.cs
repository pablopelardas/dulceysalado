using System.Collections.Generic;
using System.Threading.Tasks;
using DistriCatalogoAPI.Domain.Entities;

namespace DistriCatalogoAPI.Domain.Interfaces
{
    public interface ICompanyRepository
    {
        Task<Company> GetByIdAsync(int id);
        Task<Company> GetByCodeAsync(string codigo);
        Task<Company> GetByDomainAsync(string dominio);
        Task<IEnumerable<Company>> GetAllAsync(bool includeInactive = false);
        Task<IEnumerable<Company>> GetClientCompaniesAsync(int principalCompanyId, bool includeInactive = false);
        Task<(IEnumerable<Company> Companies, int TotalCount)> GetPagedAsync(int page, int pageSize, int? principalCompanyId = null, bool includeInactive = false);
        Task<bool> ExistsByCodeAsync(string codigo, int? excludeId = null);
        Task<bool> ExistsByDomainAsync(string dominio, int? excludeId = null);
        Task<Company> CreateAsync(Company company);
        Task UpdateAsync(Company company);
        Task<List<Company>> GetByIdsAsync(List<int> ids);
        Task<int> SaveChangesAsync();
    }
}