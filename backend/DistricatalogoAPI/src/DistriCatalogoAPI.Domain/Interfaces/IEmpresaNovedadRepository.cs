using System.Collections.Generic;
using System.Threading.Tasks;
using DistriCatalogoAPI.Domain.Entities;

namespace DistriCatalogoAPI.Domain.Interfaces
{
    public interface IEmpresaNovedadRepository
    {
        Task<EmpresaNovedad?> GetByIdAsync(int id);
        Task<EmpresaNovedad?> GetByEmpresaAndAgrupacionAsync(int empresaId, int agrupacionId);
        Task<List<EmpresaNovedad>> GetByEmpresaIdAsync(int empresaId, bool? visible = null);
        Task<(List<EmpresaNovedad> novedades, int total)> GetPagedByEmpresaAsync(
            int empresaId,
            int page,
            int pageSize,
            bool? visible = null);
        Task<EmpresaNovedad> CreateAsync(EmpresaNovedad empresaNovedad);
        Task UpdateAsync(EmpresaNovedad empresaNovedad);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int empresaId, int agrupacionId);
        
        // Métodos específicos para drag-and-drop
        Task<List<AgrupacionWithNovedadStatus>> GetAgrupacionesWithNovedadStatusAsync(int empresaId);
        Task SetNovedadesForEmpresaAsync(int empresaId, List<int> agrupacionIds);
        Task<List<Agrupacion>> GetNovedadesAgrupacionesByEmpresaAsync(int empresaId);
        Task<int> GetCountByEmpresaAsync(int empresaId);
        Task RemoveAllByEmpresaAsync(int empresaId);
    }

    public class AgrupacionWithNovedadStatus
    {
        public int Id { get; set; }
        public int Codigo { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool Activa { get; set; }
        public bool IsNovedad { get; set; }
        public int EmpresaPrincipalId { get; set; }
    }
}