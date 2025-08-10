using System.Collections.Generic;
using System.Threading.Tasks;
using DistriCatalogoAPI.Domain.Entities;

namespace DistriCatalogoAPI.Domain.Interfaces
{
    public interface IEmpresaOfertaRepository
    {
        Task<EmpresaOferta?> GetByIdAsync(int id);
        Task<EmpresaOferta?> GetByEmpresaAndAgrupacionAsync(int empresaId, int agrupacionId);
        Task<List<EmpresaOferta>> GetByEmpresaIdAsync(int empresaId, bool? visible = null);
        Task<(List<EmpresaOferta> ofertas, int total)> GetPagedByEmpresaAsync(
            int empresaId,
            int page,
            int pageSize,
            bool? visible = null);
        Task<EmpresaOferta> CreateAsync(EmpresaOferta empresaOferta);
        Task UpdateAsync(EmpresaOferta empresaOferta);
        Task DeleteAsync(int id);
        Task<bool> ExistsAsync(int empresaId, int agrupacionId);
        
        // Métodos específicos para drag-and-drop
        Task<List<AgrupacionWithOfertaStatus>> GetAgrupacionesWithOfertaStatusAsync(int empresaId);
        Task SetOfertasForEmpresaAsync(int empresaId, List<int> agrupacionIds);
        Task<List<Agrupacion>> GetOfertasAgrupacionesByEmpresaAsync(int empresaId);
        Task<int> GetCountByEmpresaAsync(int empresaId);
        Task RemoveAllByEmpresaAsync(int empresaId);
    }

    public class AgrupacionWithOfertaStatus
    {
        public int Id { get; set; }
        public int Codigo { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public bool Activa { get; set; }
        public bool IsOferta { get; set; }
        public int EmpresaPrincipalId { get; set; }
    }
}