using DistriCatalogoAPI.Domain.Entities;

namespace DistriCatalogoAPI.Application.Interfaces
{
    public interface ICurrentUserService
    {
        Task<User> GetCurrentUserAsync();
        int GetCurrentUserId();
        int GetCurrentCompanyId();
    }
}