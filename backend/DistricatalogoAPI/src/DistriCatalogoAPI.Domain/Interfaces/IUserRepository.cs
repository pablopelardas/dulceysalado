using DistriCatalogoAPI.Domain.Entities;
using DistriCatalogoAPI.Domain.ValueObjects;
using DistriCatalogoAPI.Domain.Enums;

namespace DistriCatalogoAPI.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User> GetByIdAsync(int id);
        Task<User> GetByEmailAsync(Email email, bool includeInactive = false);
        Task<User> GetByEmailOrUsernameAsync(string emailOrUsername, bool includeInactive = false);
        Task<IEnumerable<User>> GetAllByCompanyAsync(int companyId, bool includeInactive = false);
        Task<IEnumerable<User>> GetAllAsync(bool includeInactive = false);
        Task<(IEnumerable<User> Users, int TotalCount)> GetPagedAsync(int page, int pageSize, int? companyId = null, bool includeInactive = false);
        Task<bool> ExistsByEmailAsync(Email email, bool onlyActive = true);
        Task<string> GetCompanyTypeAsync(int companyId);
        Task<Company> GetCompanyAsync(int companyId);
        UserRole MapDatabaseRoleToEnum(string databaseRole, string companyType);
        Task<User> CreateAsync(User user);
        Task UpdateAsync(User user);
        Task<int> SaveChangesAsync();
    }
}