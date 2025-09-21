using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DistriCatalogoAPI.Domain.Entities;
using DistriCatalogoAPI.Domain.Interfaces;
using DistriCatalogoAPI.Infrastructure.Data;
using DistriCatalogoAPI.Infrastructure.Models;
using Microsoft.EntityFrameworkCore;

namespace DistriCatalogoAPI.Infrastructure.Persistence.Repositories
{
    public class SolicitudReventaRepository : ISolicitudReventaRepository
    {
        private readonly DistricatalogoContext _context;

        public SolicitudReventaRepository(DistricatalogoContext context)
        {
            _context = context;
        }

        public async Task<SolicitudReventa?> GetByIdAsync(int id)
        {
            return await _context.Set<SolicitudReventa>()
                .Include(s => s.Cliente)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<SolicitudReventa?> GetByClienteIdAsync(int clienteId)
        {
            return await _context.Set<SolicitudReventa>()
                .Include(s => s.Cliente)
                .Where(s => s.ClienteId == clienteId)
                .OrderByDescending(s => s.FechaSolicitud)
                .FirstOrDefaultAsync();
        }

        public async Task<IEnumerable<SolicitudReventa>> GetAllByEmpresaAsync(int empresaId)
        {
            return await _context.Set<SolicitudReventa>()
                .Include(s => s.Cliente)
                .Where(s => s.EmpresaId == empresaId)
                .OrderByDescending(s => s.FechaSolicitud)
                .ToListAsync();
        }

        public async Task<IEnumerable<SolicitudReventa>> GetPendientesAsync(int empresaId)
        {
            return await _context.Set<SolicitudReventa>()
                .Include(s => s.Cliente)
                .Where(s => s.EmpresaId == empresaId && s.Estado == EstadoSolicitud.Pendiente)
                .OrderByDescending(s => s.FechaSolicitud)
                .ToListAsync();
        }

        public async Task<SolicitudReventa> AddAsync(SolicitudReventa solicitud)
        {
            await _context.Set<SolicitudReventa>().AddAsync(solicitud);
            await _context.SaveChangesAsync();
            return solicitud;
        }

        public async Task UpdateAsync(SolicitudReventa solicitud)
        {
            solicitud.UpdatedAt = System.DateTime.UtcNow;
            _context.Set<SolicitudReventa>().Update(solicitud);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> ExistePendienteAsync(int clienteId)
        {
            return await _context.Set<SolicitudReventa>()
                .AnyAsync(s => s.ClienteId == clienteId && s.Estado == EstadoSolicitud.Pendiente);
        }
    }
}