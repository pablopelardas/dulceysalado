using System.Collections.Generic;
using System.Threading.Tasks;
using DistriCatalogoAPI.Domain.Entities;

namespace DistriCatalogoAPI.Domain.Interfaces
{
    public interface IAgrupacionRepository
    {
        Task<Agrupacion?> GetByIdAsync(int id);
        Task<Agrupacion?> GetByCodigoAsync(int codigo, int empresaPrincipalId);
        Task<List<Agrupacion>> GetByEmpresaPrincipalAsync(int empresaPrincipalId, bool includeInactive = false);
        Task<(List<Agrupacion> agrupaciones, int total)> GetPagedAsync(
            int empresaPrincipalId,
            int page,
            int pageSize,
            bool? activa = null,
            string? busqueda = null,
            int? tipo = 3);
        Task<Agrupacion> CreateAsync(Agrupacion agrupacion);
        Task UpdateAsync(Agrupacion agrupacion);
        Task<bool> ExistsByCodigoAsync(int codigo, int empresaPrincipalId, int? excludeId = null);
        Task<List<int>> GetVisibleCodigosForEmpresaAsync(int empresaId);
        Task<bool> IsVisibleForEmpresaAsync(int agrupacionId, int empresaId);
        Task SetVisibilityForEmpresaAsync(int empresaId, List<int> agrupacionIds);
        Task<int> GetCountByEmpresaPrincipalAsync(int empresaPrincipalId);
        Task<List<Agrupacion>> GetVisibleByEmpresaAsync(int empresaId);
        Task<List<Agrupacion>> GetByIdsAsync(List<int> ids);
        Task<Dictionary<int, bool>> CheckExistenceByCodigosAsync(List<int> codigos, int empresaPrincipalId);
        Task<List<Agrupacion>> CreateBulkAsync(List<Agrupacion> agrupaciones);
    }
}