using DistriCatalogoAPI.Domain.Entities;

namespace DistriCatalogoAPI.Domain.Interfaces
{
    public interface ICorreccionTokenRepository
    {
        Task<CorreccionToken?> GetByTokenAsync(string token);
        Task<CorreccionToken?> GetByIdAsync(int id);
        Task<List<CorreccionToken>> GetByPedidoIdAsync(int pedidoId);
        Task<CorreccionToken> AddAsync(CorreccionToken token);
        Task<CorreccionToken> UpdateAsync(CorreccionToken token);
        Task DeleteAsync(int id);
        Task<List<CorreccionToken>> GetExpiradosAsync();
    }
}