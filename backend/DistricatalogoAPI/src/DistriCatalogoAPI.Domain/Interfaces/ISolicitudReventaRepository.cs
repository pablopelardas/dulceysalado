using System.Collections.Generic;
using System.Threading.Tasks;
using DistriCatalogoAPI.Domain.Entities;

namespace DistriCatalogoAPI.Domain.Interfaces
{
    public interface ISolicitudReventaRepository
    {
        Task<SolicitudReventa?> GetByIdAsync(int id);
        Task<SolicitudReventa?> GetByClienteIdAsync(int clienteId);
        Task<IEnumerable<SolicitudReventa>> GetAllByEmpresaAsync(int empresaId);
        Task<IEnumerable<SolicitudReventa>> GetPendientesAsync(int empresaId);
        Task<SolicitudReventa> AddAsync(SolicitudReventa solicitud);
        Task UpdateAsync(SolicitudReventa solicitud);
        Task<bool> ExistePendienteAsync(int clienteId);
    }
}