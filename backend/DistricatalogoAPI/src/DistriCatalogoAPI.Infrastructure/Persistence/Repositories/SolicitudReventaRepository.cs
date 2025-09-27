using System;
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

        public async Task<IEnumerable<SolicitudReventa>> GetFilteredAsync(int empresaId, string? estado = null, string? search = null, int page = 1, int limit = 20, string? sortBy = "fechaSolicitud", string? sortOrder = "desc")
        {
            var query = _context.Set<SolicitudReventa>()
                .Include(s => s.Cliente)
                .Where(s => s.EmpresaId == empresaId);

            // Filtro por estado
            if (!string.IsNullOrEmpty(estado))
            {
                if (Enum.TryParse<EstadoSolicitud>(estado, out var estadoEnum))
                {
                    query = query.Where(s => s.Estado == estadoEnum);
                }
            }

            // Filtro por búsqueda (nombre cliente, razón social, CUIT, email)
            if (!string.IsNullOrEmpty(search))
            {
                query = query.Where(s =>
                    s.Cliente != null && s.Cliente.Nombre.Contains(search) ||
                    s.RazonSocial.Contains(search) ||
                    s.Cuit.Contains(search) ||
                    s.EmailComercial.Contains(search));
            }

            // Ordenamiento
            query = sortBy?.ToLower() switch
            {
                "fecha_solicitud" or "fechasolicitud" => sortOrder?.ToLower() == "asc"
                    ? query.OrderBy(s => s.FechaSolicitud)
                    : query.OrderByDescending(s => s.FechaSolicitud),
                "razon_social" or "razonsocial" => sortOrder?.ToLower() == "asc"
                    ? query.OrderBy(s => s.RazonSocial)
                    : query.OrderByDescending(s => s.RazonSocial),
                "estado" => sortOrder?.ToLower() == "asc"
                    ? query.OrderBy(s => s.Estado)
                    : query.OrderByDescending(s => s.Estado),
                "cliente_nombre" or "clientenombre" => sortOrder?.ToLower() == "asc"
                    ? query.OrderBy(s => s.Cliente != null ? s.Cliente.Nombre : "")
                    : query.OrderByDescending(s => s.Cliente != null ? s.Cliente.Nombre : ""),
                _ => sortOrder?.ToLower() == "asc"
                    ? query.OrderBy(s => s.FechaSolicitud)
                    : query.OrderByDescending(s => s.FechaSolicitud)
            };

            // Paginación
            var skip = (page - 1) * limit;
            query = query.Skip(skip).Take(limit);

            return await query.ToListAsync();
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